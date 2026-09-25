using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using DCimpossible.Content.NPCs;

namespace DCimpossible.Common.Systems
{
	public class WorldEventSystem : ModSystem
	{
		public static int DayCounter = 1;
		private static bool _lastDayTime = true;
		private static bool _day75SpawnedToday = false;
		private static bool _day200SpawnedToday = false;

		// World conversion wave tracking
		public static bool WorldConversionInProgress = false;
		public static int ConversionCurrentX = 10;
		public static int ConversionType = 1; // 1 = Corruption, 4 = Crimson

		public override void SaveWorldData(TagCompound tag)
		{
			tag["DayCounter"] = DayCounter;
			tag["Day75SpawnedToday"] = _day75SpawnedToday;
			tag["Day200SpawnedToday"] = _day200SpawnedToday;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			DayCounter = tag.GetInt("DayCounter");
			if (DayCounter <= 0) DayCounter = 1;
			_day75SpawnedToday = tag.GetBool("Day75SpawnedToday");
			_day200SpawnedToday = tag.GetBool("Day200SpawnedToday");
		}

		public override void OnWorldLoad()
		{
			_lastDayTime = Main.dayTime;
			WorldConversionInProgress = false;
		}

		public override void PreUpdateTime()
		{
			// Check day / night transitions
			if (_lastDayTime != Main.dayTime)
			{
				if (!Main.dayTime)
				{
					// Night began: Blood Moon every night
					Main.bloodMoon = true;
					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.WorldData);
					}
				}
				else
				{
					// Day began: Increment day counter & handle day events
					DayCounter++;
					_day75SpawnedToday = false;
					_day200SpawnedToday = false;
					Main.bloodMoon = false;

					if (Main.hardMode)
					{
						Main.eclipse = true;
					}
					else
					{
						Main.eclipse = false;
						if (Main.invasionType == 0)
						{
							Main.StartInvasion(InvasionID.GoblinArmy);
						}
					}

					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.WorldData);
					}

					CheckSpecialDaySpawns();
				}

				_lastDayTime = Main.dayTime;
			}

			// Continuously guarantee active events
			if (!Main.dayTime)
			{
				if (!Main.bloodMoon)
				{
					Main.bloodMoon = true;
				}
			}
			else
			{
				if (Main.hardMode)
				{
					if (!Main.eclipse)
					{
						Main.eclipse = true;
					}
				}
				else
				{
					if (Main.invasionType == 0)
					{
						Main.StartInvasion(InvasionID.GoblinArmy);
					}
				}
			}

			// Handle world conversion wave if in progress
			UpdateWorldConversionWave();
		}

		private static void CheckSpecialDaySpawns()
		{
			// Day 75 Apocalypse (every 75 days)
			if (DayCounter > 0 && DayCounter % 75 == 0 && !_day75SpawnedToday)
			{
				_day75SpawnedToday = true;
				TriggerDay75BossApocalypse();
			}

			// Day 200 Cataclysmic Deity & World Devourment
			if (DayCounter >= 200 && !_day200SpawnedToday)
			{
				_day200SpawnedToday = true;
				TriggerDay200DeityEvent();
			}
		}

		private static void TriggerDay75BossApocalypse()
		{
			BroadcastMessage("THE APOCALYPSE HAS ARRIVED: ALL BOSSES HAVE AWOKEN!", Color.Red);

			Player target = Main.player.FirstOrDefault(p => p.active && !p.dead);
			if (target == null) return;

			int[] bosses = new int[]
			{
				NPCID.KingSlime,
				NPCID.EyeofCthulhu,
				NPCID.EaterofWorldsHead,
				NPCID.BrainofCthulhu,
				NPCID.QueenBee,
				NPCID.SkeletronHead,
				NPCID.Deerclops,
				NPCID.WallofFlesh,
				NPCID.Retinazer,
				NPCID.Spazmatism,
				NPCID.TheDestroyer,
				NPCID.SkeletronPrime,
				NPCID.QueenSlimeBoss,
				NPCID.Plantera,
				NPCID.Golem,
				NPCID.DukeFishron,
				NPCID.HallowBoss,
				NPCID.CultistBoss,
				NPCID.MoonLordCore
			};

			foreach (int bossId in bosses)
			{
				int spawnX = (int)target.Center.X + Main.rand.Next(-500, 500);
				int spawnY = (int)target.Center.Y + Main.rand.Next(-600, -200);
				int idx = NPC.NewNPC(new EntitySource_WorldEvent(), spawnX, spawnY, bossId);
				if (Main.netMode == NetmodeID.Server && idx < Main.maxNPCs)
				{
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, idx);
				}
			}
		}

		public static void TriggerDay200DeityEvent()
		{
			BroadcastMessage("REALITY SHATTERS. THE CATACLYSMIC DEITY HAS DESCENDED TO CLAIM YOUR REALM!", new Color(150, 0, 255));

			Player target = Main.player.FirstOrDefault(p => p.active && !p.dead);
			if (target != null)
			{
				int deityType = ModContent.NPCType<CataclysmicDeity>();
				int idx = NPC.NewNPC(new EntitySource_WorldEvent(), (int)target.Center.X, (int)target.Center.Y - 400, deityType);
				if (Main.netMode == NetmodeID.Server && idx < Main.maxNPCs)
				{
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, idx);
				}
			}

			// Start progressive world conversion
			WorldConversionInProgress = true;
			ConversionCurrentX = 10;
			ConversionType = WorldGen.crimson ? 4 : 1;
		}

		private static void UpdateWorldConversionWave()
		{
			if (!WorldConversionInProgress) return;

			// Process 15 columns per tick to smoothly transform the entire world without game freeze
			int columnsPerTick = 15;
			int endX = Math.Min(ConversionCurrentX + columnsPerTick, Main.maxTilesX - 10);

			for (int x = ConversionCurrentX; x < endX; x += 10)
			{
				for (int y = 10; y < Main.maxTilesY - 10; y += 10)
				{
					WorldGen.Convert(x, y, ConversionType, 12, true, true);
				}
			}

			ConversionCurrentX = endX;
			if (ConversionCurrentX >= Main.maxTilesX - 10)
			{
				WorldConversionInProgress = false;
				BroadcastMessage("YOUR ENTIRE WORLD HAS BEEN CORRUPTED FOR ALL ETERNITY.", Color.Purple);
			}
		}

		public static void BroadcastMessage(string text, Color color)
		{
			if (Main.netMode == NetmodeID.Server)
			{
				Terraria.Chat.ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral(text), color);
			}
			else
			{
				Main.NewText(text, color);
			}
		}
	}
}
