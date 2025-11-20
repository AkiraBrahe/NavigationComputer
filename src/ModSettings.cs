using System.Collections.Generic;

namespace NavigationComputer
{
    public class ModSettings
    {
        public MapVisualSettings MapVisuals { get; set; } = new MapVisualSettings();
        public Dictionary<string, string> SearchableTags = [];
    }

    public class MapVisualSettings
    {
        public bool HighlightInhabitedSystems { get; set; } = true;
        public bool ShowPopulationLevels { get; set; } = true;
        public bool ResizeStarClusters { get; set; } = true;
    }
}
