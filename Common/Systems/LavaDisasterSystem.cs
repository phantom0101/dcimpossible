using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using DCimpossible.Content.Projectiles;

namespace DCimpossible.Common.Systems
{
	public class LavaDisasterSystem : ModSystem
	{
		public static bool LavaRainActive = false;
		public static int LavaRainTimeLeft = 0;

		public override void PostUpdateTime()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) return;

			// Handle Lava Rain disaster
			UpdateLavaRain();

			// Handle Lava Tsunami disaster
			UpdateLavaTsunami();
		}

		private static void UpdateLavaRain()
		{
			if (!LavaRainActive)
			{
				// 1 in 18,000 chance per tick (~every 5 minutes)
				if (Main.rand.Next(18000) == 0)
				{
					LavaRainActive = true;
					LavaRainTimeLeft = Main.rand.Next(3600, 7200); // 1-2 minutes
					WorldEventSystem.BroadcastMessage("A SCORCHING LAVA RAIN HAS BEGUN TO MELT THE SURFACE!", Color.OrangeRed);
				}
			}
			else
			{
				LavaRainTimeLeft--;
				if (LavaRainTimeLeft <= 0)
				{
					LavaRainActive = false;
					WorldEventSystem.BroadcastMessage("The scorching lava rain has subsided.", Color.Orange);
					return;
				}

				// Spawn falling lava drops around active players
				foreach (Player player in Main.player)
				{
					if (!player.active || player.dead) continue;

					// Spawn drops continuously (every ~3-4 ticks)
					if (Main.rand.NextBool(3))
					{
						float spawnX = player.Center.X + Main.rand.NextFloat(-900f, 900f);
						float spawnY = player.Center.Y - 650f;
						Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(9f, 15f));

						int proj = Projectile.NewProjectile(
							new EntitySource_WorldEvent(),
							spawnX,
							spawnY,
							velocity.X,
							velocity.Y,
							ModContent.ProjectileType<LavaRainDrop>(),
							150,
							3f,
							Main.myPlayer
						);
						if (Main.netMode == NetmodeID.Server && proj < Main.maxProjectiles)
						{
							NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
						}
					}
				}
			}
		}

		private static void UpdateLavaTsunami()
		{
			// 1 in 24,000 chance per tick (~every 6.6 minutes)
			if (Main.rand.Next(24000) == 0)
			{
				WorldEventSystem.BroadcastMessage("WARNING: A COLOSSAL LAVA TSUNAMI IS SWEEPING ACROSS THE LAND!", Color.Red);

				foreach (Player player in Main.player)
				{
					if (!player.active || player.dead) continue;

					int direction = Main.rand.NextBool() ? 1 : -1;
					float startX = player.Center.X - (direction * 1100f);
					float startY = player.Center.Y - 30f;
					Vector2 velocity = new Vector2(direction * 13f, 0f);

					// Tower of 3 wave segments spanning vertically
					for (int i = 0; i < 3; i++)
					{
						int proj = Projectile.NewProjectile(
							new EntitySource_WorldEvent(),
							startX,
							startY - (i * 130f),
							velocity.X,
							velocity.Y,
							ModContent.ProjectileType<LavaTsunamiWave>(),
							300,
							8f,
							Main.myPlayer
						);
						if (Main.netMode == NetmodeID.Server && proj < Main.maxProjectiles)
						{
							NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
						}
					}
					break; // Spawn 1 tsunami wave per event
				}
			}
		}
	}
}

