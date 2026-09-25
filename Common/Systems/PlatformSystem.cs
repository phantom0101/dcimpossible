using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DCimpossible.Common.Systems
{
	public class PlatformSystem : ModPlayer
	{
		public override void PostUpdate()
		{
			if (Player.dead) return;

			// Check tiles under player's feet
			int startX = (int)(Player.position.X / 16f);
			int endX = (int)((Player.position.X + Player.width) / 16f);
			int tileY = (int)((Player.position.Y + Player.height + 2f) / 16f);

			for (int x = startX; x <= endX; x++)
			{
				if (x < 0 || x >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY) continue;

				Tile tile = Main.tile[x, tileY];
				if (tile.HasTile && TileID.Sets.Platforms[tile.TileType])
				{
					// Check if properly reinforced:
					// 1. Has a background wall behind it
					// 2. Or has a solid block/beam/support directly underneath
					bool hasWall = tile.WallType != WallID.None;
					bool hasSupportUnderneath = tileY + 1 < Main.maxTilesY && Main.tile[x, tileY + 1].HasTile && !TileID.Sets.Platforms[Main.tile[x, tileY + 1].TileType];

					if (!hasWall && !hasSupportUnderneath)
					{
						// Unreinforced platform! Chance to break per tick while standing/running on it (1 in 15 ticks ~ 4 times a sec)
						if (Main.rand.Next(15) == 0)
						{
							BreakPlatform(x, tileY);
						}
					}
				}
			}
		}

		private static void BreakPlatform(int x, int y)
		{
			SoundEngine.PlaySound(SoundID.Dig, new Vector2(x * 16, y * 16));
			for (int i = 0; i < 6; i++)
			{
				Dust.NewDust(new Vector2(x * 16, y * 16), 16, 16, DustID.WoodFurniture, 0f, 2f);
			}

			WorldGen.KillTile(x, y, fail: false);
			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(-1, x, y, 1);
			}
		}
	}
}
