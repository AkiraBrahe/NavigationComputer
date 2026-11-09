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

    /// <summary>
    /// Filters out flashpoints that have been skipped from the list of available flashpoints.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "CheckForNewFlashpoints")]
    public static class SimGameState_CheckForNewFlashpoints
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return new CodeMatcher(instructions, il)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt,
                    AccessTools.Method(typeof(List<FlashpointDef>), nameof(List<>.FindAll))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(FlashpointSkip), nameof(FlashpointSkip.FilterFlashpointCandidates))))
                .InstructionEnumeration();
        }
    }

    /// <summary>
    /// Filters out flashpoints that have been skipped from the list of completed flashpoints.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.GenerateFlashpoint))]
    public static class SimGameState_GenerateFlashpoint
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return new CodeMatcher(instructions, il)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld,
                    AccessTools.Field(typeof(SimGameState), nameof(SimGameState.completedFlashpoints))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(FlashpointSkip), nameof(FlashpointSkip.AddSkippedFlashpoints))))
                .InstructionEnumeration();
        }
    }
}