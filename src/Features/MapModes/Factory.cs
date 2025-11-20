using BattleTech;
using BattleTech.UI;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Factory map mode: Highlights systems with faction stores.
    /// </summary>
    public class Factory : IMapMode
    {
        public string Name { get; } = "Factory Systems";

        public void Apply(SimGameState simGame) { }

        public void Unapply(SimGameState simGame) { }

        internal static void HideBlackMarketIndicators(StarmapSystemRenderer systemRenderer)
        {
            if (systemRenderer.currentBlackMarketObj != null)
            {
                systemRenderer.SetBlackMarket(false);
            }
        }
        internal static void HidePulseOnNonAlliedStores(SGNavigationScreen navScreen, StarmapSystemRenderer systemRenderer)
        {
            var system = systemRenderer.system.System;
            var owner = system.Def.FactionShopOwnerValue.IsInvalidUnset ? system.Def.OwnerValue : system.Def.FactionShopOwnerValue;

            if (systemRenderer.currentFactionObj != null && !navScreen.simState.IsFactionAlly(owner, null))
            {
                var techPulse = systemRenderer.currentFactionObj.transform.Find("techPulse");
                techPulse?.gameObject.SetActive(false);
            }
        }
    }
}