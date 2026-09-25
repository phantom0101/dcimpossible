using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DCimpossible.Common.Items
{
	public class DCItemGlobal : GlobalItem
	{
		public override void SetDefaults(Item item)
		{
			// Feature 3: Base item defense reduced by 75%
			if (item.defense > 0)
			{
				item.defense = Math.Max(1, (int)Math.Round(item.defense * 0.25f));
			}
		}

		public override void OnSpawn(Item item, IEntitySource source)
		{
			// Feature 10: EVERY drop in game is 1% chance or less, including boss drops
			// Only gate actual world drops (NPC loot, boss bags, tile breaks, trees, crates)
			// Do NOT delete player's manual drops or 5-min fumble drops
			if (source is EntitySource_Loot ||
				source is EntitySource_ItemOpen ||
				source is EntitySource_TileBreak ||
				source is EntitySource_ShakeTree ||
				source is EntitySource_Caught ||
				source is EntitySource_FishedOut ||
				source is EntitySource_BossSpawn)
			{
				// 99% chance to turn to air (1% drop rate or less)
				if (Main.rand.Next(100) != 0)
				{
					item.TurnToAir();
					item.active = false;
				}
			}
		}

		public override bool CanUseItem(Item item, Player player)
		{
			// Feature 13: Disable every infinite flight tool
			if (IsInfiniteFlightTool(item.type))
			{
				Main.NewText($"The power of {item.Name} has been suppressed! Infinite flight is forbidden.", Color.Red);
				return false;
			}

			return base.CanUseItem(item, player);
		}

		public override bool? UseItem(Item item, Player player)
		{
			// Feature 15: Tools have a chance to break
			if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
			{
				// 1 in 150 chance per swing (~0.67%)
				if (Main.rand.Next(150) == 0)
				{
					string toolName = item.Name;
					item.TurnToAir();
					SoundEngine.PlaySound(SoundID.Item37, player.position); // Metallic clang / snap
					SoundEngine.PlaySound(SoundID.Shatter, player.position);
					Main.NewText($"Your {toolName} snapped and broke completely!", Color.OrangeRed);
					return true;
				}
			}

			return base.UseItem(item, player);
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (IsInfiniteFlightTool(item.type))
			{
				tooltips.Add(new TooltipLine(Mod, "DCFlightDisabled", "[DISABLED: Infinite flight forbidden]")
				{
					OverrideColor = Color.Red
				});
			}

			if (item.defense > 0)
			{
				tooltips.Add(new TooltipLine(Mod, "DCDefenseNerf", "[DCimpossible: Armor reduced by 75%]")
				{
					OverrideColor = Color.IndianRed
				});
			}

			if (item.damage > 0 && !item.accessory)
			{
				tooltips.Add(new TooltipLine(Mod, "DCDamageNerf", "[DCimpossible: Damage reduced by 75%]")
				{
					OverrideColor = Color.IndianRed
				});
			}

			if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
			{
				tooltips.Add(new TooltipLine(Mod, "DCToolDurability", "[Fragile: Can break during use]")
				{
					OverrideColor = Color.Goldenrod
				});
			}
		}

		public static bool IsInfiniteFlightTool(int type)
		{
			return type == ItemID.EmpressFlightBooster || // Soaring Insignia
				   type == ItemID.CosmicCarKey ||          // UFO Mount
				   type == ItemID.WitchBroom ||            // Witch's Broom Mount
				   type == ItemID.ShrimpyTruffle ||        // Cute Fishron Mount
				   type == ItemID.PirateShipMountItem ||    // The Black Spot Mount
				   type == ItemID.DrillContainmentUnit;    // DCU Mount
		}
	}
}

