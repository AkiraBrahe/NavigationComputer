using BattleTech;
using NavigationComputer.Features;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Turns off any active map mode when changing rooms in the dropship.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "SetSimRoomState")]
    public static class SimGameState_SetSimRoomState_Patch
    {
        public static void Prefix(DropshipLocation state)
        {
            if (state != DropshipLocation.NAVIGATION)
                MapModesUI.TurnMapModeOff();
        }
    }
}
