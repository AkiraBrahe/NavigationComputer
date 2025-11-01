using BattleTech;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Unvisited map mode: Dims star systems that have already been visited.
    /// </summary>
    public class Unvisited(float dimLevel = 10f) : IMapMode
    {
        private readonly float _dimLevel = dimLevel;

        public string Name { get; } = "Unvisited Systems";

        public void Apply(SimGameState simGame)
        {
            var visitedSystems = simGame.VisitedStarSystems;
            foreach (string system in visitedSystems)
                MapModesUI.DimSystem(system, _dimLevel);
        }

        public void Unapply(SimGameState simGame)
        {
        }
    }
}
