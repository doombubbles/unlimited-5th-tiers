﻿using System.Linq;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Input;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using Unlimited5thTiers;

[assembly: MelonInfo(typeof(Unlimited5thTiersMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Unlimited5thTiers;

public class Unlimited5thTiersMod : BloonsTD6Mod
{
    public override void OnNewGameModel(GameModel gameModel, List<ModModel> mods)
    {
        foreach (var superMonkey in gameModel.GetTowersWithBaseId(TowerType.SuperMonkey))
        {
            if (superMonkey.GetDescendant<MonkeyTempleModel>() is { towerGroupCount: < 4 } monkeyTempleModel)
            {
                monkeyTempleModel.towerGroupCount = 4;
            }
        }
    }

    [HarmonyPatch(typeof(TowerManager), nameof(TowerManager.IsTowerPathTierLocked))]
    internal class TowerManager_IsTowerPathTierLocked
    {
        [HarmonyPostfix]
        internal static void Postfix(TowerManager __instance, ref bool __result)
        {
            if (Settings.AllowUnlimited5thTiers)
            {
                __result = false;
            }
        }
    }

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
                var superMonkeys = __instance.Sim.towerManager.GetTowersByBaseId(TowerType.SuperMonkey).ToList()
                    .Where(tower => tower.Id != __instance.tower.Id).ToList();
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

    [HarmonyPatch(typeof(MonkeyTemple), nameof(MonkeyTemple.StartSacrifice))]
    public class MonkeyTemple_StartSacrifice
    {
        [HarmonyPostfix]
        public static void Postfix(MonkeyTemple __instance)
        {
            if (__instance.monkeyTempleModel.checkForThereCanOnlyBeOne && !__instance.checkTCBOO)
            {
                __instance.checkTCBOO = true;
            }
        }
    }


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

    [HarmonyPatch(typeof(TowerInventory), nameof(TowerInventory.IsPathTierLocked))]
    internal static class TowerInventory_IsPathTierLocked
    {
        [HarmonyPrefix]
        private static bool Prefix(int tier, ref bool __result)
        {
            if (tier == 5)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}