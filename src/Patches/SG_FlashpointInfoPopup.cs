using BattleTech.UI;
using NavigationComputer.Features;

namespace NavigationComputer.Patches
{
    /// <summary>
    /// Updates the flashpoint info popup when it is refreshed.
    /// </summary>
    [HarmonyPatch(typeof(SG_FlashpointInfoPopup), "Refresh")]
    public static class SG_FlashpointInfoPopup_Refresh
    {
        [HarmonyPostfix]
        public static void Postfix(SG_FlashpointInfoPopup __instance)
        {
            var updater = __instance.GetComponent<FlashpointSkipButton>()
                ?? __instance.gameObject.AddComponent<FlashpointSkipButton>();
            updater.Initialize(__instance);

            FlashpointSkip.UpdateSkipTooltip(__instance);
        }
    }

    /// <summary>
    /// Skips the flashpoint if the control key is held down when the confirm button is pressed.
    /// </summary>
    [HarmonyPatch(typeof(SG_FlashpointInfoPopup), "OnConfirm")]
    public static class SG_FlashpointInfoPopup_OnConfirm
    {
        [HarmonyPrefix]
        public static bool Prefix(SG_FlashpointInfoPopup __instance)
        {
            if (!FlashpointSkipButton.ControlHeld)
                return true;

            var flashpoint = FlashpointSkipButton.GetFlashpoint(__instance);
            var simGame = FlashpointSkipButton.GetSimGame(__instance);
            if (flashpoint == null || simGame == null
                || !simGame.AvailableFlashpoints.Contains(flashpoint)
                || flashpoint == simGame.ActiveFlashpoint)
                return true;

            if (!FlashpointSkip.CanSkipFlashpoint(flashpoint, simGame))
                return false;

            if (FlashpointSkip.SkipFlashpoint(flashpoint, simGame))
                __instance.ForceClose();
            return false;
        }
    }
}
