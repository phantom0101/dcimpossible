using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DCimpossible.Content.Projectiles
{
	public class LavaRainDrop : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_664"; // Volcanic boulder / fiery projectile texture

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
			Projectile.ignoreWater = false;
		}

		public override void AI()
		{
			// Fiery trail
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f);
				d.noGravity = true;
				d.scale = 1.3f;
			}
			Lighting.AddLight(Projectile.Center, 1f, 0.4f, 0.1f);

			// Accelerate downwards
			Projectile.velocity.Y += 0.25f;
			if (Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.position);
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position); // Explosion / sizzle

			int tileX = (int)(Projectile.Center.X / 16f);
			int tileY = (int)(Projectile.Center.Y / 16f);

			// Melt / destroy blocks in a small radius (1-2 tiles)
			for (int x = tileX - 1; x <= tileX + 1; x++)
			{
				for (int y = tileY - 1; y <= tileY + 1; y++)
				{
					if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY) continue;

					Tile tile = Main.tile[x, y];
					if (tile.HasTile)
					{
						// Protect containers with items to prevent world/save corruption
						if (TileID.Sets.BasicChest[tile.TileType] || tile.TileType == TileID.Containers || tile.TileType == TileID.Containers2)
						{
							continue;
						}

						WorldGen.KillTile(x, y, fail: false);
						if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.SendTileSquare(-1, x, y, 1);
						}
					}
				}
			}

			// Burst of lava droplets
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-4f, 0f), 0, default, 1.6f);
			}

			return true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.OnFire3, 300); // Hellfire for 5 seconds
		}
	}
}

