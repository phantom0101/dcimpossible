using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using DCimpossible.Common.Systems;

namespace DCimpossible.Common.Players
{
	public class DCPlayer : ModPlayer
	{
		public int DropTimer = 0;
		public const int DropIntervalTicks = 18000; // 5 minutes at 60 ticks/second

		public override void OnEnterWorld()
		{
			EnforceHardcore();
			BegoneEvilDetector.CheckAndPunish(Player);
		}

		public override void OnRespawn()
		{
			BegoneEvilDetector.CheckAndPunish(Player);
		}

		public override void PreUpdate()
		{
			EnforceHardcore();
		}

		private void EnforceHardcore()
		{
			// Player difficulty: 0 = Classic, 1 = Mediumcore, 2 = Hardcore
			if (Player.difficulty != 2)
			{
				Player.difficulty = 2;
				Main.NewText("Hardcore mode strictly enforced for DCimpossible!", Color.Red);
			}
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
		{
			// Feature 3: Weapon damage lowered by 75% (only 25% effective)
			damage *= 0.25f;
		}

		public override void PostUpdateEquips()
		{
			// Feature 3: Armor / total defense lowered by 75%
			Player.statDefense.FinalMultiplier *= 0.25f;

			// Feature 13: Disable Soaring Insignia infinite flight
			Player.empressBrooch = false;
		}

		public override void PostUpdate()
		{
			if (Player.dead) return;

			// Feature 13: Disable infinite flight mounts
			if (Player.mount.Active)
			{
				int mountType = Player.mount.Type;
				if (mountType == MountID.UFO ||
					mountType == MountID.WitchBroom ||
					mountType == MountID.CuteFishron ||
					mountType == MountID.PirateShip ||
					mountType == MountID.Drill)
				{
					Player.mount.Dismount(Player);
					Main.NewText("Infinite flight mounts are forbidden in DCimpossible!", Color.OrangeRed);
				}
			}

			// Feature 11: Every 5 minutes a random inventory item drops (entire stack)
			// Server-authoritative: only runs on server or in singleplayer.
			// In multiplayer, the server simulates all players so this fires once
			// per player per interval — not once per client per player.
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				DropTimer++;
				if (DropTimer >= DropIntervalTicks)
				{
					DropTimer = 0;
					DropRandomInventoryItem();
				}
			}

			// Feature 12: Random tripping while running on the ground
			// Server-authoritative: prevents each client rolling independently
			// and firing the trip at different times.
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				CheckTripping();
			}
		}

		private void DropRandomInventoryItem()
		{
			List<int> validSlots = new List<int>();
			// Check inventory slots 0 to 49 (hotbar + backpack)
			for (int i = 0; i < 50; i++)
			{
				if (!Player.inventory[i].IsAir && Player.inventory[i].stack > 0)
				{
					validSlots.Add(i);
				}
			}

			if (validSlots.Count > 0)
			{
				int slot = validSlots[Main.rand.Next(validSlots.Count)];
				Item itemToDrop = Player.inventory[slot].Clone();
				Player.inventory[slot].TurnToAir();

				// QuickSpawnItem is preferred for player drops:
				// it sets the correct noGrabDelay and auto-syncs to all clients in MP.
				Player.QuickSpawnItem(Player.GetSource_DropAsItem(), itemToDrop, itemToDrop.stack);
				SoundEngine.PlaySound(SoundID.Item16, Player.position);

				// Sync the cleared inventory slot to all clients.
				// MessageID.SyncEquipment is the correct message for Player.inventory[] slots.
				// number = player whoAmI, number2 = slot index (must be float).
				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Player.whoAmI, (float)slot);
				}

				WorldEventSystem.BroadcastMessage(
					$"[{Player.name}] Clumsy hands! Dropped an entire stack of {itemToDrop.Name}!",
					Color.OrangeRed);
			}
		}

		private void CheckTripping()
		{
			// Only trip if sprinting/moving horizontally on solid ground without grappling
			if (Math.Abs(Player.velocity.X) > 1.5f && Player.velocity.Y == 0f && Player.grapCount == 0 && !Player.mount.Active)
			{
				// 1 in 15,000 chance per tick (~once every 4.1 minutes of continuous running)
				if (Main.rand.Next(15000) == 0)
				{
					TriggerTrip();
				}
			}
		}

		private void TriggerTrip()
		{
			// Stumble physics — the server sets velocity which is synced to the client
			// via the normal player position/velocity sync packets.
			Player.velocity = new Vector2(-Player.direction * 5f, -3.5f);
			SoundEngine.PlaySound(SoundID.DoubleJump, Player.position);
			SoundEngine.PlaySound(SoundID.Shatter, Player.position);

			// Drop all inventory, coin (50-53) and ammo (54-57) slots as world items.
			// QuickSpawnItem handles noGrabDelay and auto-syncs the spawned entity to clients.
			for (int i = 0; i < 58; i++)
			{
				Item inv = Player.inventory[i];
				if (!inv.IsAir && inv.stack > 0)
				{
					Player.QuickSpawnItem(Player.GetSource_DropAsItem(), inv, inv.stack);
					Player.inventory[i].TurnToAir();

					// Sync the cleared slot to all clients.
					// number2 must be cast to float for the SyncEquipment message.
					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Player.whoAmI, (float)i);
					}
				}
			}

			WorldEventSystem.BroadcastMessage(
				$"[{Player.name}] TRIPPED and scattered EVERYTHING from their inventory!",
				Color.Red);
		}
	}
}

