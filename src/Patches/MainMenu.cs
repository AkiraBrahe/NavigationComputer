using BattleTech.UI;
using Flashpoint = NavigationComputer.Features.MapModes.Flashpoint;

namespace NavigationComputer.Patches
{
    [HarmonyPatch(typeof(MainMenu), "Init")]
    public static class MainMenu_Init_Patch
    {
        [HarmonyPostfix]
        public static void Postfix() => Flashpoint.IncreaseMaximumFlashpointCount();
    }
}