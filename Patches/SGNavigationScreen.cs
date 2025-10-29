using BattleTech;
using BattleTech.UI;
using NavigationComputer.Features;
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
}
