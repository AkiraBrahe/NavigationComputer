using BattleTech;
using NavigationComputer.Features;
using NavigationComputer.Features.MapModes;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Turns off any active map mode when changing rooms in the dropship.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "SetSimRoomState")]
    public static class SimGameState_SetSimRoomState
    {
        [HarmonyPrefix]
        public static void Prefix(DropshipLocation state)
        {
            if (state != DropshipLocation.NAVIGATION)
                MapModesUI.TurnMapModeOff();
        }
    }

    /// <summary>
    /// Adds searchable tags to pirate haven systems.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "InitializeDataFromDefs")]
    public static class SimGameState_InitializeDataFromDefs
    {
        [HarmonyPostfix]
        public static void Postfix(SimGameState __instance)
        {
            BlackMarket.AddPirateHavenTags(__instance.DataManager);
        }
    }
}