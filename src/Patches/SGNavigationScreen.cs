using BattleTech;
using BattleTech.UI;
using NavigationComputer.Features;
using NavigationComputer.Features.MapModes;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NavigationComputer.Patches
{
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
                    MapModesUI.ToggleMapMode(key);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
                MapModesUI.StartSearching();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                MapModesUI.StartSearching(initialQuery: "target:comstar");
        }
    }

    /// <summary>
    /// Initializes the map modes UI when the navigation screen is opened.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "Init", typeof(SimGameState), typeof(SGRoomController_Navigation))]
    public static class SGNavigationScreen_Init
    {
        [HarmonyPostfix]
        public static void Postfix(SGNavigationScreen __instance, SimGameState simGame)
        {
            MapModesUI.SetupUIObjects(__instance);
            MapModesUI.SimGame = simGame;
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
            if (MapModesUI.CurrentMapMode == null)
            {
                __runOriginal = true;
                return;
            }

            MapModesUI.TurnMapModeOff();
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
                .InstructionEnumeration();
        }

        [HarmonyPostfix]
        public static void Postfix(SGNavigationScreen __instance, string systemID)
        {
            if (!Factory.IsActive) return;

            var simGame = __instance.simState;
            var systemRenderer = simGame.Starmap.Screen.GetSystemRenderer(systemID);
            if (systemRenderer != null)
            {
                Factory.HidePulseOnNonAlliedStores(__instance, simGame, systemRenderer);
            }
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
        public static void Postfix(SGNavigationScreen __instance, string systemID)
        {
            if (!BlackMarket.IsActive) return;

            var systemRenderer = __instance.simState.Starmap.Screen.GetSystemRenderer(systemID);
            if (systemRenderer != null)
            {
                BlackMarket.ShowPirateHavenIndicators(__instance, systemID, systemRenderer);
            }
        }
    }
}