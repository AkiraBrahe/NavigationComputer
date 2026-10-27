using System;
using System.Collections.Generic;

namespace NavigationComputer
{
    public class ModSettings
    {
        public MapModeSettings MapModes { get; set; } = new MapModeSettings();
        public MapVisualSettings MapVisuals { get; set; } = new MapVisualSettings();
        public Dictionary<string, string> SearchableTags = [];
        public Dictionary<string, CampaignSettings> LoreCampaigns = [];
    }

    public class CampaignSettings
    {
        public string[] BranchIds { get; set; } = [];
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MapModeSettings
    {
        public bool ShowFlashpointTracker { get; set; } = true;
        public bool HideFutureFlashpointNames { get; set; } = true;
    }

    public class MapVisualSettings
    {
        public bool HighlightInhabitedSystems { get; set; } = true;
        public bool ShowPopulationLevels { get; set; } = true;
        public bool HighlightStarClusters { get; set; } = true;
    }
}