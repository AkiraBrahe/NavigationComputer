using BattleTech;
using BattleTech.UI;
using NavigationComputer.Features;
using NavigationComputer.Features.MapModes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NavigationComputer.Features.IndicatorFilter;
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
            SetupFilterDropdown(__instance);
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
    /// Filters special system indicators based on the current filter settings and map mode.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "GetSystemSpecialIndicator")]
    public static class SGNavigationScreen_GetSystemSpecialIndicator
    {
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.High)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            return new CodeMatcher(instructions, il)
                .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SimGameState), "IsSystemFactionStore")))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IndicatorFilter), nameof(ShouldShowFactionStore))))

                .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SimGameState), "IsSystemBlackMarket")))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IndicatorFilter), nameof(ShouldShowBlackMarketIndicator))))

                .MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SimGameState), "IsFactionAlly")))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IndicatorFilter), nameof(ShouldShowFactoryIndicator))))
                .InstructionEnumeration();
        }
    }

    /// <summary>
    /// Hides black market indicators when the factory map mode is active
    /// and hides the pulse effect on non-allied faction stores.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "GetSystemSpecialIndicator")]
    public static class SGNavigationScreen_GetSystemSpecialIndicator_Factory
    {
        [HarmonyPostfix]
        public static void Postfix(SGNavigationScreen __instance, StarmapSystemRenderer __result)
        {
            if (CurrentMapMode is Factory && __result != null)
            {
                Factory.HideBlackMarketIndicators(__result);
                Factory.HidePulseOnNonAlliedStores(__instance, __result);
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
        public static void Postfix(SGNavigationScreen __instance, StarmapSystemRenderer __result)
        {
            if (CurrentMapMode is BlackMarket && __result != null)
            {
                BlackMarket.ShowPirateHavenIndicators(__instance.specialIndicatorSystems, __result.system.System.ID, __result);
            }
        }
    }

    /// <summary>
    /// Hides flashpoint indicators based on the filter setting.
    /// </summary>
    [HarmonyPatch(typeof(SGNavigationScreen), "GetSystemFlashpoint")]
    public static class SGNavigationScreen_GetSystemFlashpoint
    {
        [HarmonyPrefix]
        public static bool Prefix(ref StarmapSystemRenderer __result)
        {
            if (CurrentMapMode is null && !ShowFlashpoints)
            {
                __result = null;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Fixes a crash caused by Colourful Flashpoints when fashpoint indicators are disabled.
    /// </summary>
    [HarmonyPatch]
    public static class SGNavigationScreen_GetSystemFlashpoint_CFPFix
    {
        [HarmonyPrepare]
        public static bool Prepare() => AppDomain.CurrentDomain.GetAssemblies().Any(asm => asm.GetName().Name.Equals("ColourfulFlashPoints"));

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            var type = Type.GetType("ColourfulFlashPoints.Patches.SGNavigationScreen_GetSystemFlashpoint, ColourfulFlashPoints");
            return type?.GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var matcher = new CodeMatcher(instructions, il);
            var continueLabel = il.DefineLabel();

            matcher.Start().AddLabels([continueLabel]);
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldind_Ref),
                new CodeInstruction(OpCodes.Brtrue, continueLabel),
                new CodeInstruction(OpCodes.Ret)
            );

            return matcher.InstructionEnumeration();
        }
    }
}