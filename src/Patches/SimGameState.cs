using BattleTech;
using NavigationComputer.Features;
using NavigationComputer.Features.MapModes;
using System.Collections.Generic;
using System.Reflection.Emit;
using Flashpoint = NavigationComputer.Features.MapModes.Flashpoint;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Refreshes flashpoint data and applies skip expiration policy when loading a save.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    internal class SimGameState_RehydratePatch
    {
        [HarmonyPostfix]
        public static void Postfix(SimGameState __instance)
        {
            Flashpoint.RefreshFlashpointData();
            FlashpointSkip.ApplySkipExpirationPolicy(__instance);
            Main.Log.LogDebug($"Loading skipped flashpoints: {string.Join(",", Main.CampaignState.SkippedFlashpoints.Keys)}");
        }
    }

    /// <summary>
    /// Adds searchable tags to pirate haven systems when loading a save.
    /// </summary>
    [HarmonyPatch(typeof(StarSystem), "Rehydrate")]
    public static class StarSystem_Rehydrate
    {
        [HarmonyPostfix]
        public static void Postfix(StarSystem __instance) => BlackMarket.AddPirateHavenTags(__instance);
    }

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
    /// Filters out flashpoints that have been skipped from the list of available flashpoints.
    /// </summary>
    [HarmonyPatch(typeof(SimGameState), "CheckForNewFlashpoints")]
    public static class SimGameState_CheckForNewFlashpoints
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return new CodeMatcher(instructions, il)
                .MatchEndForward(new CodeMatch(OpCodes.Callvirt,
                    AccessTools.Method(typeof(List<FlashpointDef>), nameof(List<>.FindAll))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(FlashpointSkip), nameof(FlashpointSkip.FilterFlashpointCandidates))))
                .InstructionEnumeration();
        }
    }
}