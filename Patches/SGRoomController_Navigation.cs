using BattleTech.UI;
using NavigationComputer.Features;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Turns off any active map mode when exiting the navigation screen.
    /// </summary>
    [HarmonyPatch(typeof(SGRoomController_Navigation), "ExitNavScreen")]
    public static class SGRoomController_Navigation_ExitNavScreen_Patch
    {
        public static void Prefix() => MapModesUI.TurnMapModeOff();
    }
}
