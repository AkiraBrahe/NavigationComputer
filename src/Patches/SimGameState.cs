using BattleTech;
using NavigationComputer.Features;
using NavigationComputer.Features.MapModes;
using Flashpoint = NavigationComputer.Features.MapModes.Flashpoint;

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
    [HarmonyPatch(typeof(StarSystem), "Rehydrate")]
    public static class StarSystem_Rehydrate
    {
        [HarmonyPostfix]
        public static void Postfix(StarSystem __instance) => BlackMarket.AddPirateHavenTags(__instance);
    }

    /// <summary>
    /// Refreshes flashpoint data when loading a save.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "InitFromSave")]
    public static class SimGameState_InitFromSave
    {
        [HarmonyPostfix]
        public static void Postfix() => Flashpoint.RefreshFlashpointData();
    }
}