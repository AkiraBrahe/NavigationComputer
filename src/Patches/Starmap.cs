using BattleTech;
using NavigationComputer.Features;
using UnityEngine;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Handles shift-click system selection to extend the current path.
    /// </summary>
    [HarmonyPatch(typeof(Starmap), "SetSelectedSystem", typeof(StarSystemNode))]
    public static class Starmap_SetSelectedSystem_Patch
    {
        public static void Prefix(ref bool __runOriginal, Starmap __instance, StarSystemNode node)
        {
            if (!__runOriginal) return;
            bool result = ShiftClickMove.HandleClickSystem(__instance, node);
            if (!result)
            {
                __runOriginal = false;
                return;
            }

            __runOriginal = true;
            return;
        }
    }

    /// <summary>
    /// Detects if shift is held during system selection to mark the next selection as a shift-click.
    /// </summary>
    [HarmonyPatch(typeof(Starmap), "GetLocationByUV")]
    public static class Starmap_GetLocationByUV_Patch
    {
        public static void Postfix(Starmap __instance, StarSystemNode __result)
        {
            if (__result != null && __result != __instance.CurSelected && __result != __instance.CurPlanet
                && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                ShiftClickMove.NextSelectIsShiftClick = true;
            }
        }
    }
}
