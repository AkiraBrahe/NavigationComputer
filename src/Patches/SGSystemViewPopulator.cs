using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using NavigationComputer.Features.MapModes;
using UnityEngine;

namespace NavigationComputer.Patches
{
    [HarmonyPatch(typeof(SGSystemViewPopulator), "UpdateRoutedSystem")]
    public static class SGSystemViewPopulator_UpdateRoutedSystem_Patch
    {
        public static void Postfix(SGSystemViewPopulator __instance)
        {
            if (__instance.BlackMarketlSystemIndicator != null && __instance.BlackMarketlSystemIndicator.activeSelf)
            {
                BlackMarket.ShowBlackMarketOwner(__instance.starSystem, __instance.BlackMarketlSystemIndicator, __instance.BlackMarketTooltip);
            }
        }
    }
}