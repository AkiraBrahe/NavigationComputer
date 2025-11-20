using BattleTech;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Difficulty map mode: Scales star systems based on their difficulty.
    /// </summary>
    public class Difficulty : IMapMode
    {
        public string Name { get; } = "System Difficulty";

        public void Apply(SimGameState simGame)
        {
            foreach (string system in simGame.StarSystemDictionary.Keys)
            {
                var starSystem = simGame.StarSystemDictionary[system];
                int difficulty = starSystem.Def.GetDifficulty(simGame.SimGameMode);

                MapModesUI.ScaleSystem(system, difficulty / 5f);
            }
        }

        public void Unapply(SimGameState simGame)
        {
            foreach (string system in simGame.StarSystemDictionary.Keys)
                MapModesUI.ScaleSystem(system, 0.5f);
        }
    }
}
