using BattleTech.UI;
using NavigationComputer.Features.MapModes;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Displays the black market owner info on the system view when applicable.
    /// </summary>
    [HarmonyPatch(typeof(SGSystemViewPopulator), "UpdateRoutedSystem")]
    public static class SGSystemViewPopulator_UpdateRoutedSystem
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