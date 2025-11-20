using BattleTech;
using BattleTech.UI;
using NavigationComputer.Features;
using NavigationComputer.Features.MapModes;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NavigationComputer.Features.MapModesUI;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Initializes the map modes UI when the navigation screen is opened.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "Init", typeof(SimGameState), typeof(SGRoomController_Navigation))]
    public static class SGNavigationScreen_Init
    {
        [HarmonyPostfix]
        public static void Postfix(SGNavigationScreen __instance, SimGameState simGame)
        {
            SimGame = simGame;
            SetupUIObjects(__instance);
        }
    }

    /// <summary>
    /// Handles input for toggling map modes and starting searches.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "Update")]
    public static class SGNavigationScreen_Update
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            foreach (var key in MapModesUI.DiscreteMapModes.Keys)
            {
                if (Input.GetKeyUp(key))
                    ToggleMapMode(key);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
                StartSearching();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                StartSearching(initialQuery: "target:comstar");
        }
    }

    /// <summary>
    /// Handles the Escape key to turn off active map modes.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "HandleEscapeKeypress")]
    public static class SGNavigationScreen_HandleEscapeKeypress
    {
        [HarmonyPrefix]
        public static void Prefix(ref bool __runOriginal, ref bool __result)
        {
            if (!__runOriginal) return;
            if (CurrentMapMode is null)
            {
                __runOriginal = true;
                return;
            }

            TurnMapModeOff();
            __result = true;
            __runOriginal = false;
            return;
        }
    }

    /// <summary>
    /// Shows the faction store indicators when the factory map mode is active
    /// and hides the pulse effect on non-allied faction stores.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "GetSystemSpecialIndicator")]
    public static class SGNavigationScreen_GetSystemSpecialIndicator_Factory
    {
        [HarmonyPrepare]
        public static bool Prepare() => !Main.BTFactionStoreUnlockDetected;

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return new CodeMatcher(instructions, il)
                .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SimGameState), "IsFactionAlly")))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Factory), nameof(Factory.ShouldShowFactionStoreIcon))))

                .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SimGameState), "IsSystemBlackMarket")))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Factory), nameof(Factory.ShouldShowBlackMarketIndicator))))
                .InstructionEnumeration();
        }

        [HarmonyPostfix]
        public static void Postfix(SGNavigationScreen __instance, StarmapSystemRenderer __result)
        {
            if (!Factory.IsActive || __result == null) return;

            Factory.HidePulseOnNonAlliedStores(__instance, __instance.simState, __result);
        }
    }

    /// <summary>
    /// Shows pirate haven indicators when the black market map mode is active.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "GetSystemSpecialIndicator")]
    public static class SGNavigationScreen_GetSystemSpecialIndicator_BlackMarket
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(SGNavigationScreen __instance, StarmapSystemRenderer __result)
        {
            if (!BlackMarket.IsActive || __result == null) return;

            BlackMarket.ShowPirateHavenIndicators(__instance.specialIndicatorSystems, __result.system.System.ID, __result);
        }
    }
}