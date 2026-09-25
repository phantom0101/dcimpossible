using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using DCimpossible.Common.Systems;
using DCimpossible.Content.NPCs;

namespace DCimpossible.Common.NPCs
{
	public class DCNPCGlobal : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool BuffApplied = false;
		private static bool _isSummoningMoonLordAdditions = false;

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			ApplyStats(npc);

			// Feature 9: When summoning Moon Lord -> 2 Moon Lords, 1 Duke Fishron, 1 random boss
			if (npc.type == NPCID.MoonLordCore && !_isSummoningMoonLordAdditions)
			{
				_isSummoningMoonLordAdditions = true;
				try
				{
					// Spawn 2nd Moon Lord Core
					int ml2 = NPC.NewNPC(source, (int)npc.Center.X + 300, (int)npc.Center.Y, NPCID.MoonLordCore);
					if (Main.netMode == NetmodeID.Server && ml2 < Main.maxNPCs)
					{
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, ml2);
					}

					// Spawn Duke Fishron
					int duke = NPC.NewNPC(source, (int)npc.Center.X, (int)npc.Center.Y - 250, NPCID.DukeFishron);
					if (Main.netMode == NetmodeID.Server && duke < Main.maxNPCs)
					{
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, duke);
					}

					// Spawn 1 random boss
					int[] randomBossPool = new int[]
					{
						NPCID.Plantera,
						NPCID.Golem,
						NPCID.HallowBoss,
						NPCID.CultistBoss,
						NPCID.Retinazer,
						NPCID.TheDestroyer,
						NPCID.SkeletronPrime,
						NPCID.QueenBee,
						NPCID.Deerclops,
						NPCID.KingSlime,
						NPCID.EyeofCthulhu
					};
					int chosenBoss = Main.rand.Next(randomBossPool);
					int randBossIdx = NPC.NewNPC(source, (int)npc.Center.X - 300, (int)npc.Center.Y, chosenBoss);
					if (Main.netMode == NetmodeID.Server && randBossIdx < Main.maxNPCs)
					{
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, randBossIdx);
					}

					WorldEventSystem.BroadcastMessage("MOON LORD HAS SUMMONED REINFORCEMENTS! (2 Moon Lords, Duke Fishron, and a mystery boss)", Color.Cyan);
				}
				finally
				{
					_isSummoningMoonLordAdditions = false;
				}
			}
		}

		public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
		{
			ApplyStats(npc);
		}

		private void ApplyStats(NPC npc)
		{
			if (BuffApplied) return;
			if (npc.friendly || npc.type == NPCID.TargetDummy || npc.lifeMax <= 5) return;
			if (npc.type == ModContent.NPCType<CataclysmicDeity>()) return;

			BuffApplied = true;

			// Base +500% HP (6x)
			float hpMult = 6f;

			// Pre-Hardmode boss +300% HP (4x) -> 6x * 4x = 24x
			if (IsPreHardmodeBoss(npc.type))
			{
				hpMult *= 4f;
			}

			long newLife = (long)(npc.lifeMax * hpMult);
			if (newLife > int.MaxValue) newLife = int.MaxValue;
			npc.lifeMax = (int)newLife;
			npc.life = npc.lifeMax;
		}

		public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
		{
			if (npc.friendly) return;
			if (npc.type == ModContent.NPCType<CataclysmicDeity>()) return;

			// Base +500% damage (6x)
			float dmgMult = 6f;

			// Pre-Hardmode boss +300% damage (4x) -> 6x * 4x = 24x
			if (IsPreHardmodeBoss(npc.type))
			{
				dmgMult *= 4f;
			}

			modifiers.SourceDamage *= dmgMult;
		}

		public override void PostAI(NPC npc)
		{
			// Feature 4: Worm AI enemies 150% faster (+150% = 2.5x speed)
			if (npc.aiStyle == NPCAIStyleID.Worm || IsWormSegment(npc.type))
			{
				npc.position += npc.velocity * 1.5f;
			}
		}

		public static bool IsPreHardmodeBoss(int type)
		{
			return type == NPCID.KingSlime ||
				   type == NPCID.EyeofCthulhu ||
				   type == NPCID.EaterofWorldsHead ||
				   type == NPCID.EaterofWorldsBody ||
				   type == NPCID.EaterofWorldsTail ||
				   type == NPCID.BrainofCthulhu ||
				   type == NPCID.Creeper ||
				   type == NPCID.QueenBee ||
				   type == NPCID.SkeletronHead ||
				   type == NPCID.SkeletronHand ||
				   type == NPCID.Deerclops ||
				   type == NPCID.WallofFlesh ||
				   type == NPCID.WallofFleshEye ||
				   type == NPCID.TheHungry ||
				   type == NPCID.TheHungryII;
		}

		public static bool IsWormSegment(int type)
		{
			return type == NPCID.TheDestroyer ||
				   type == NPCID.TheDestroyerBody ||
				   type == NPCID.TheDestroyerTail ||
				   type == NPCID.EaterofWorldsHead ||
				   type == NPCID.EaterofWorldsBody ||
				   type == NPCID.EaterofWorldsTail ||
				   type == NPCID.WyvernHead ||
				   type == NPCID.WyvernBody ||
				   type == NPCID.WyvernBody2 ||
				   type == NPCID.WyvernBody3 ||
				   type == NPCID.WyvernTail ||
				   type == NPCID.DiggerHead ||
				   type == NPCID.DiggerBody ||
				   type == NPCID.DiggerTail ||
				   type == NPCID.TombCrawlerHead ||
				   type == NPCID.TombCrawlerBody ||
				   type == NPCID.TombCrawlerTail ||
				   type == NPCID.DuneSplicerHead ||
				   type == NPCID.DuneSplicerBody ||
				   type == NPCID.DuneSplicerTail ||
				   type == NPCID.BloodEelHead ||
				   type == NPCID.BloodEelBody ||
				   type == NPCID.BloodEelTail;
		}
	}

	public class DCProjectileGlobal : GlobalProjectile
	{
		public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
		{
			if (projectile.hostile)
			{
				modifiers.SourceDamage *= 6f;
			}
		}
	}
}

