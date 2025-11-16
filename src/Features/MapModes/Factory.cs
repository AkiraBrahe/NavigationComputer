using BattleTech;
using BattleTech.UI;
using System.Collections.Generic;

namespace NavigationComputer.Features.MapModes
{
    public class Factory : IMapMode
    {
        public static bool IsActive { get; private set; }

        public string Name { get; } = "Factory Systems";

        public void Apply(SimGameState simGame)
        {
            IsActive = true;
        }

        public void Unapply(SimGameState simGame)
        {
            IsActive = false;
        }

        internal static void HidePulseOnNonAlliedStores(SGNavigationScreen navigationScreen, SimGameState simGame, StarmapSystemRenderer systemRenderer)
        {
            var system = systemRenderer.system.System;
            var owner = system.Def.FactionShopOwnerValue.IsInvalidUnset ? system.Def.OwnerValue : system.Def.FactionShopOwnerValue;

            if (simGame.IsSystemFactionStore(system, owner) && !navigationScreen.simState.IsFactionAlly(owner, null) && systemRenderer.currentFactionObj != null)
            {
                var techPulse = systemRenderer.currentFactionObj.transform.Find("techPulse");
                techPulse?.gameObject.SetActive(false);
            }
        }

        internal static bool ShouldShowFactionStoreIcon(SimGameState simGame, FactionValue faction, List<string> allyListOverride) =>
            IsActive || simGame.IsFactionAlly(faction, allyListOverride);
    }
}