using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Simulation.Input;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using Unlimited5thTiers;

[assembly: MelonInfo(typeof(Unlimited5thTiersMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Unlimited5thTiers;

public class Unlimited5thTiersMod : BloonsTD6Mod
{
    /// <summary>
    /// Sun Temples with 4 Sacrifice Groups
    /// </summary>
    public override void OnNewGameModel(GameModel gameModel)
    {
        foreach (var superMonkey in gameModel.GetTowersWithBaseId(TowerType.SuperMonkey))
        {
            if (superMonkey.GetDescendant<MonkeyTempleModel>() is { towerGroupCount: < 4 } monkeyTempleModel)
            {
                monkeyTempleModel.towerGroupCount = 4;
            }
        }

        foreach (var towerModel in gameModel.towers.Where(model => model.tier >= 5))
        {
            towerModel.GetDescendants<LimitProjectileModel>().ForEach(model => model.globalForPlayer = false);
        }
    }

    /// <summary>
    /// Unlimited 5th Tiers
    /// </summary>
    [HarmonyPatch(typeof(TowerInventory), nameof(TowerInventory.SetTowerTierRestrictions))]
    internal static class TowerInventory_SetTowerTierRestrictions
    {
        [HarmonyPostfix]
        private static void Postfix(TowerInventory __instance, IEnumerable<TowerDetailsModel> towers)
        {
            if (!Settings.AllowUnlimited5thTiers) return;

            towers.ForEach(towerDetails =>
            {
                if (towerDetails.Is<ShopTowerDetailsModel>())
                {
                    for (var path = 0; path < 3; path++)
                    {
                        __instance.AddTierRestriction(towerDetails.towerId, path, 5, 9999999);
                    }
                }
            });
        }
    }

    /// <summary>
    /// Non-maxed Vengeful Temples
    /// </summary>
    [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.StartSacrifice))]
    public class MonkeyTemple_StartSacrifice
    {
        [HarmonyPostfix]
        public static void Postfix(MonkeyTemple __instance)
        {
            if (Settings.AllowNonMaxedVTSGs &&
                __instance.monkeyTempleModel.checkForThereCanOnlyBeOne &&
                !__instance.checkTCBOO)
            {
                __instance.checkTCBOO = true;
            }
        }
    }

    /// <summary>
    /// Multiple Vengeful Temples
    /// </summary>
    [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.CheckTCBOO))]
    internal class MonkeyTemple_CheckTCBOO
    {
        [HarmonyPrefix]
        internal static bool Prefix(MonkeyTemple __instance)
        {
            if (Settings.AllowUnlimitedVTSGs &&
                __instance.checkTCBOO &&
                __instance.monkeyTempleModel.weaponDelayFrames + __instance.lastSacrificed <=
                __instance.Sim.time.elapsed &&
                __instance.monkeyTempleModel.checkForThereCanOnlyBeOne &&
                __instance.lastSacrificed != __instance.Sim.time.elapsed)
            {
                var superMonkeys = __instance.Sim.towerManager
                    .GetTowersByBaseId(TowerType.SuperMonkey)
                    .ToList()
                    .Where(tower => tower.Id != __instance.tower.Id)
                    .ToList<Tower>();
                var robocop = superMonkeys.FirstOrDefault(tower => tower.towerModel.tiers[1] == 5);
                var batman = superMonkeys.FirstOrDefault(tower => tower.towerModel.tiers[2] == 5);
                if (batman != default && robocop != default)
                {
                    __instance.SacrificeBatmanAndRoboCop(robocop, batman);
                }

                __instance.checkTCBOO = false;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Unlimited / Sandbox Paragons
    /// </summary>
    [HarmonyPatch(typeof(Tower), nameof(Tower.CanUpgradeToParagon))]
    internal class Tower_CanUpgradeToParagon
    {
        [HarmonyPostfix]
        internal static void Postfix(Tower __instance, ref bool __result)
        {
            if (__instance.Sim.towerManager.IsParagonLocked(__instance, __instance.owner) ||
                __instance.towerModel.paragonUpgrade == null ||
                !Settings.AllowUnlimitedParagons)
            {
                return;
            }

            var towers = __instance.Sim.towerManager.GetTowersByBaseId(__instance.towerModel.baseId).ToList();
            for (var i = 0; i < 3; i++)
            {
                if (towers.All(tower => tower.towerModel.tiers[i] != 5))
                {
                    return;
                }
            }

            __result = true;
        }
    }

    /// <summary>
    /// Unlimited Paragons
    /// </summary>
    [HarmonyPatch(typeof(Tower), nameof(Tower.HasReachedParagonLimit))]
    internal static class Tower_HasParagonLimitBeenReached
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            if (Settings.AllowUnlimitedParagons)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}