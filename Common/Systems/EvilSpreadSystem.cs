using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DCimpossible.Common.Systems
{
	public class EvilSpreadSystem : ModSystem
	{
		public override void PostUpdateWorld()
		{
			// Vanilla runs ~ (Main.maxTilesX * Main.maxTilesY) * 3e-5f iterations per tick
			// For evil spread to be 300% faster (4x total), we run 3x additional targeted infection passes
			int iterations = (int)((Main.maxTilesX * Main.maxTilesY) * 9e-5f);
			if (iterations < 200) iterations = 200;
			if (iterations > 1500) iterations = 1500;

			for (int n = 0; n < iterations; n++)
			{
				int x = Main.rand.Next(10, Main.maxTilesX - 10);
				int y = Main.rand.Next(10, Main.maxTilesY - 20);

				if (Main.hardMode)
				{
					WorldGen.hardUpdateWorld(x, y);
				}

				Tile tile = Main.tile[x, y];
				if (!tile.HasTile) continue;

				ushort type = tile.TileType;
				// Corruption spread
				if (type == TileID.CorruptGrass || type == TileID.Ebonstone || type == TileID.Ebonsand || 
					type == TileID.CorruptIce || type == TileID.CorruptHardenedSand || type == TileID.CorruptSandstone)
				{
					WorldGen.SpreadInfectionToNearbyTile(x, y, 1, 4);
				}
				// Crimson spread
				else if (type == TileID.CrimsonGrass || type == TileID.Crimstone || type == TileID.Crimsand || 
						 type == TileID.FleshIce || type == TileID.CrimsonHardenedSand || type == TileID.CrimsonSandstone)
				{
					WorldGen.SpreadInfectionToNearbyTile(x, y, 4, 4);
				}
			}
		}
	}
}

