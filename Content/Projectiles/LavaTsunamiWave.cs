using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DCimpossible.Content.Projectiles
{
	public class LavaTsunamiWave : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_664";

		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 160;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 500;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			// Continuous roaring lava wall particles
			for (int i = 0; i < 4; i++)
			{
				Vector2 dustPos = Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height));
				Dust d = Dust.NewDustDirect(dustPos, 4, 4, DustID.Lava, Projectile.velocity.X * 0.4f, Main.rand.NextFloat(-3f, 1f), 0, default, 2f);
				d.noGravity = true;

				Dust smoke = Dust.NewDustDirect(dustPos, 4, 4, DustID.Smoke, Projectile.velocity.X * 0.2f, -2f, 100, default, 1.8f);
				smoke.noGravity = true;
			}

			Lighting.AddLight(Projectile.Center, 1.5f, 0.6f, 0.1f);

			// Sound effect periodically
			if (Projectile.timeLeft % 60 == 0)
			{
				SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.Center);
			}

			// Place occasional lava in hollow ground
			if (Projectile.timeLeft % 20 == 0)
			{
				int tileX = (int)(Projectile.Center.X / 16f);
				int tileY = (int)(Projectile.Bottom.Y / 16f);
				if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY - 1)
				{
					Tile bottomTile = Main.tile[tileX, tileY];
					Tile floorTile = Main.tile[tileX, tileY + 1];
					if (!bottomTile.HasTile && floorTile.HasTile && bottomTile.LiquidAmount < 200)
					{
						bottomTile.LiquidType = LiquidID.Lava;
						bottomTile.LiquidAmount = 255;
						WorldGen.SquareTileFrame(tileX, tileY);
						if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.sendWater(tileX, tileY);
						}
					}
				}
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.velocity.X += Math.Sign(Projectile.velocity.X) * 12f;
			target.velocity.Y -= 6f;
			target.AddBuff(BuffID.OnFire3, 600); // 10 seconds of Hellfire
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = texture.Size() / 2f;

			for (int y = -60; y <= 60; y += 30)
			{
				Color waveColor = new Color(255, 120, 30, 220);
				Main.EntitySpriteDraw(texture, drawPos + new Vector2(0, y), null, waveColor, Projectile.rotation, origin, new Vector2(2f, 2.5f), SpriteEffects.None, 0);
			}

			return false;
		}
	}
}
