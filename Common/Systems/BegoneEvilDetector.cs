using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DCimpossible.Common.Systems
{
	public class BegoneEvilDetector : ModSystem
	{
		private static bool? _detected;

		public static bool IsBegoneEvilDetected
		{
			get
			{
				if (!_detected.HasValue)
				{
					_detected = DetectBegoneEvil();
				}
				return _detected.Value;
			}
		}

		private static bool DetectBegoneEvil()
		{
			if (ModLoader.HasMod("BegoneEvil") || ModLoader.HasMod("Begone_Evil"))
			{
				return true;
			}

			foreach (var mod in ModLoader.Mods)
			{
				if (mod == null) continue;
				string name = mod.Name ?? string.Empty;
				string displayName = mod.DisplayName ?? string.Empty;

				if (name.IndexOf("BegoneEvil", StringComparison.OrdinalIgnoreCase) >= 0 ||
					name.IndexOf("Begone_Evil", StringComparison.OrdinalIgnoreCase) >= 0 ||
					displayName.IndexOf("Begone Evil", StringComparison.OrdinalIgnoreCase) >= 0 ||
					displayName.IndexOf("BegoneEvil", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}

			return false;
		}

		public static void CheckAndPunish(Player player)
		{
			if (player.dead) return;

			if (IsBegoneEvilDetected)
			{
				Main.NewText("ALERT: 'Begone, Evil!' mod detected!", Color.Red);
				Main.NewText("You must disable 'Begone, Evil!' immediately. Cowardice is not tolerated in DCimpossible!", Color.OrangeRed);

				player.KillMe(
					PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral($"{player.name} was obliterated for refusing to disable Begone Evil!")),
					999999,
					0
				);
			}
		}
	}
}
