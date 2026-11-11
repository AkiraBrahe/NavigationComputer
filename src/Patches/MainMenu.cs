using BattleTech.UI;
using NavigationComputer.Features;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Increases the maximum flashpoint count on game start.
    /// </summary>
    [HarmonyPatch(typeof(MainMenu), "Init")]
    public static class MainMenu_Init
    {
        [HarmonyPostfix]
        public static void Postfix() => FlashpointSkip.IncreaseMaximumFlashpointCount();
    }
}