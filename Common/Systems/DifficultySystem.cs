using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DCimpossible.Common.Systems
{
	public class DifficultySystem : ModSystem
	{
		public override void OnWorldLoad()
		{
			EnforceMasterMode();
		}

		public override void PreUpdateWorld()
		{
			EnforceMasterMode();
		}

		private static void EnforceMasterMode()
		{
			// GameMode: 0 = Classic, 1 = Expert, 2 = Master, 3 = Journey
			if (Main.GameMode != 2)
			{
				Main.GameMode = 2;
				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
	}
}

