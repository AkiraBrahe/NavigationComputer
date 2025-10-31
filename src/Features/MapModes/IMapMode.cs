using BattleTech;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Interface for map modes that can be applied to the star map.
    /// </summary>
    public interface IMapMode
    {
        string Name { get; }
        void Apply(SimGameState simGameState);
        void Unapply(SimGameState simGameState);
    }
}
