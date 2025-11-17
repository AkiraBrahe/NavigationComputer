using BattleTech;
using System.Collections.Generic;
using System.Linq;

namespace NavigationComputer.Features.MapModes
{
    public class Flashpoint(float dimLevel = 5f) : IMapMode
    {
        public readonly HashSet<string> FlashpointSystemIds = [];

        private readonly float _dimLevel = dimLevel;

        public string Name { get; } = "Active Flashpoints";

        public void Apply(SimGameState simGame)
        {
            FlashpointSystemIds.Clear();

            foreach (var flashpoint in simGame.AvailableFlashpoints)
            {
                if (flashpoint.CurStatus != BattleTech.Flashpoint.Status.WAITING_FOR_DATA)
                {
                    FlashpointSystemIds.Add(flashpoint.CurSystem.ID);
                }
            }

            HighlightFlashpointSystems(simGame);
        }

        public void Unapply(SimGameState simGame)
        {
        }

        private void HighlightFlashpointSystems(SimGameState simGame)
        {
            var allSystems = simGame.StarSystemDictionary.Values.ToList();
            foreach (var system in allSystems)
            {
                if (!FlashpointSystemIds.Contains(system.ID))
                {
                    MapModesUI.DimSystem(system.ID, _dimLevel, true);
                }
            }
        }
    }
}
