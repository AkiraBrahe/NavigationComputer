using BattleTech;
using BattleTech.UI;
using NavigationComputer.Features;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Handles input for toggling map modes and starting searches.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "Update")]
    public static class SGNavigationScreen_Update_Patch
    {
        public static void Postfix()
        {
            foreach (var key in MapModesUI.DiscreteMapModes.Keys)
            {
                if (Input.GetKeyUp(key))
                    MapModesUI.ToggleMapMode(key);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
                MapModesUI.StartSearching();
        }
    }

    /// <summary>
    /// Initializes the map modes UI when the navigation screen is opened.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "Init", typeof(SimGameState), typeof(SGRoomController_Navigation))]
    public static class SGNavigationScreen_Init_Patch
    {
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
    public static class SGNavigationScreen_HandleEscapeKeypress_Patch
    {
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
    public static class SGNavigationScreen_GetSystemSpecialIndicator_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return new CodeMatcher(instructions, il)
                .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SimGameState), "IsFactionAlly")))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SGNavigationScreen_GetSystemSpecialIndicator_Patch), "ShouldShowFactionStoreIcon")))
                .InstructionEnumeration();
        }

        public static bool ShouldShowFactionStoreIcon(SimGameState sim, FactionValue faction, List<string> allyListOverride) =>
            Features.MapModes.Factory.IsActive || sim.IsFactionAlly(faction, allyListOverride);

        [HarmonyPostfix]
        public static void Postfix(SGNavigationScreen __instance, string systemID)
        {
            if (!Features.MapModes.Factory.IsActive) return;

            var simState = __instance.simState;
            var renderer = simState.Starmap.Screen.GetSystemRenderer(systemID);
            var system = renderer.system.System;
            if (renderer == null || system == null) return;

            var owner = system.Def.FactionShopOwnerValue.IsInvalidUnset ? system.Def.OwnerValue : system.Def.FactionShopOwnerValue;
            if (simState.IsSystemFactionStore(system, owner) && !simState.IsFactionAlly(owner, null) && renderer.currentFactionObj != null)
            {
                var techPulse = renderer.currentFactionObj.transform.Find("techPulse");
                techPulse?.gameObject.SetActive(false);
            }
        }
    }
}
