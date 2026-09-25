using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DCimpossible.Content.NPCs
{
	[AutoloadBossHead]
	public class CataclysmicDeity : ModNPC
	{
		public override string Texture => "Terraria/Images/NPC_398"; // Moon Lord Core texture base

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 1;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.MPAllowedEnemies[Type] = true;
		}

		public override void SetDefaults()
		{
			NPC.width = 160;
			NPC.height = 160;
			NPC.damage = 999999;
			NPC.defense = 9999;
			NPC.lifeMax = 1000000000;
			NPC.life = NPC.lifeMax;
			NPC.dontTakeDamage = true; // Complete invincibility
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.aiStyle = -1; // Custom AI
			NPC.npcSlots = 50f;
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
		{
			NPC.lifeMax = 1000000000;
			NPC.life = NPC.lifeMax;
			NPC.dontTakeDamage = true;
		}

		public override void AI()
		{
			Player target = Main.player[NPC.target];
			if (!target.active || target.dead)
			{
				NPC.TargetClosest(true);
				target = Main.player[NPC.target];
			}

			// Dark cosmic visual effects
			if (Main.rand.NextBool(2))
			{
				int dustType = WorldGen.crimson ? DustID.CrimsonTorch : DustID.PurpleTorch;
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType, 0f, 0f, 100, default, 2.5f);
				d.velocity = NPC.velocity * 0.5f + Main.rand.NextVector2Circular(4f, 4f);
				d.noGravity = true;
			}

			if (target != null && target.active && !target.dead)
			{
				// Relentless pursuit
				Vector2 destination = target.Center + new Vector2(0f, -120f);
				Vector2 direction = destination - NPC.Center;
				float speed = 14f;
				float inertia = 20f;

				if (direction.Length() > 20f)
				{
					direction.Normalize();
					direction *= speed;
					NPC.velocity = (NPC.velocity * (inertia - 1) + direction) / inertia;
				}

				// Periodic attack barrages
				NPC.ai[0]++;
				if (NPC.ai[0] >= 60f) // Every 1 second
				{
					NPC.ai[0] = 0f;
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Vector2 shootVel = Vector2.Normalize(target.Center - NPC.Center) * 12f;
						int proj = Projectile.NewProjectile(
							NPC.GetSource_FromAI(),
							NPC.Center,
							shootVel,
							ProjectileID.PhantasmalEye,
							50000,
							5f,
							Main.myPlayer
						);
						Main.projectile[proj].hostile = true;
						Main.projectile[proj].friendly = false;
					}
				}
			}
			else
			{
				NPC.velocity.Y -= 0.1f;
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			// Absolute 1-hit kill
			modifiers.SourceDamage.Flat += 999999;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			target.KillMe(PlayerDeathReason.ByNPC(NPC.whoAmI), 999999, 0);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			// Draw ominous dark/red divine pulsating aura
			Color deityColor = WorldGen.crimson ? new Color(255, 30, 30, 200) : new Color(180, 50, 255, 200);
			Texture2D texture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
			Vector2 drawPos = NPC.Center - screenPos;
			Vector2 origin = texture.Size() / 2f;

			for (int i = 0; i < 4; i++)
			{
				Vector2 offset = new Vector2(0, 4).RotatedBy(Main.GlobalTimeWrappedHourly * 4f + i * MathHelper.PiOver2);
				spriteBatch.Draw(texture, drawPos + offset, null, deityColor * 0.6f, NPC.rotation, origin, NPC.scale * 1.08f, SpriteEffects.None, 0f);
			}

			spriteBatch.Draw(texture, drawPos, null, Color.White, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}

