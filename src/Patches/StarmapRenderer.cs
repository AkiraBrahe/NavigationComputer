using BattleTech;
using NavigationComputer.Features;
using UnityEngine;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Detects if shift is held during system renderer selection to mark the next selection as a shift-click.
    /// </summary>
    [HarmonyPatch(typeof(StarmapRenderer), "SetSelectedSystemRenderer")]
    public static class StarmapRenderer_SetSelectedSystemRenderer
    {
        [HarmonyPrefix]
        public static void Prefix(ref bool __runOriginal, StarmapRenderer __instance, StarmapSystemRenderer systemRenderer)
        {
            if (!__runOriginal) return;
            if (systemRenderer == null && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                Main.Log.LogDebug("Skipping SetSelectedSystemRenderer with systemRenderer null while shift held");
                __runOriginal = false;
                return;
            }

            if (systemRenderer != null && systemRenderer != __instance.currSystem
                                       && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                ShiftClickMove.NextSelectIsShiftClick = true;
            }

            __runOriginal = true;
            return;
        }
    }
}
