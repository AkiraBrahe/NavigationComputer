using BattleTech;

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
    }
}