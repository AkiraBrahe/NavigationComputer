using BattleTech;
using BattleTech.Data;
using BattleTech.UI.Tooltips;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Black Market map mode: Highlights star systems controlled by criminal and pirate factions.
    /// </summary>
    public class BlackMarket(float dimLevel = 5f) : IMapMode
    {
        #region Fields and Properties

        public static readonly Dictionary<string, BlackMarketData> BlackMarketFactions = new()
        {
            ["CriminalBeroskiFamily"] = new BlackMarketData { FriendlyName = "Beroski Family", PirateHavenID = "Montcoal", BlackMarketName = "Davion", BlackMarketColor = "#ffdc6b" },
            ["CriminalCloak"] = new BlackMarketData { FriendlyName = "Cloak Syndicate", PirateHavenID = "Baxley", BlackMarketName = "Davion", BlackMarketColor = "#ffdc6b" },
            ["CriminalMalthus"] = new BlackMarketData { FriendlyName = "Malthus Syndicate", PirateHavenID = "Tharkad", BlackMarketName = "Steiner", BlackMarketColor = "#4169e1" },
            ["CriminalManTLE"] = new BlackMarketData { FriendlyName = "Man-TLE", PirateHavenID = "AlbertFalls", BlackMarketName = "Marik", BlackMarketColor = "#c892ff" },
            ["CriminalRedCobraTriad"] = new BlackMarketData { FriendlyName = "Red Cobra Triad", PirateHavenID = "Solaris", BlackMarketName = "Solaris", BlackMarketColor = "#336495" },
            ["CriminalRostakovTong"] = new BlackMarketData { FriendlyName = "Rostakov Tong", PirateHavenID = "OldKentucky", BlackMarketName = "March", BlackMarketColor = "#7e9db2" },
            ["CriminalYakuza"] = new BlackMarketData { FriendlyName = "Yakuza", PirateHavenID = "Luthien", BlackMarketName = "Kurita", BlackMarketColor = "#dc143c" },
            ["CriminalYizhiTong"] = new BlackMarketData { FriendlyName = "Yizhi Tong", PirateHavenID = "Kittery", BlackMarketName = "Liao", BlackMarketColor = "#d0ff90" },

            ["PiratesDamned"] = new BlackMarketData { FriendlyName = "Antallos Pirates", PirateHavenID = "Antallos(PortKrin)", BlackMarketName = "Damned", BlackMarketColor = "#f04228" },
            ["PiratesAurigan"] = new BlackMarketData { FriendlyName = "Aurigan Pirates", PirateHavenID = "Herotitus", BlackMarketName = "Aurigan", BlackMarketColor = "#e95c4b" },
            ["PiratesBelt"] = new BlackMarketData { FriendlyName = "Belt Pirates", PirateHavenID = "StarsEnd(NovoCressidas)", BlackMarketName = "Coreward", BlackMarketColor = "#87851c" },
            ["PiratesMarch"] = new BlackMarketData { FriendlyName = "Chaos March Pirates", PirateHavenID = "", BlackMarketName = "March", BlackMarketColor = "#7e9db2" },
            ["PiratesCircinus"] = new BlackMarketData { FriendlyName = "Circinus Pirates", PirateHavenID = "Circinus", BlackMarketName = "Circinus", BlackMarketColor = "#7e51cc" },
            ["PiratesMarian"] = new BlackMarketData { FriendlyName = "Marian Pirates", PirateHavenID = "Alphard(MH)", BlackMarketName = "Marian", BlackMarketColor = "#f78549" },
            ["PiratesOberon"] = new BlackMarketData { FriendlyName = "Oberon Pirates", PirateHavenID = "Oberon", BlackMarketName = "Coreward", BlackMarketColor = "#87851c" },
            ["PiratesExtractor"] = new BlackMarketData { FriendlyName = "Rim Worlds Pirates", PirateHavenID = "Dijonne(Pain3050+)", BlackMarketName = "Extractor", BlackMarketColor = "#2850c7" },
            ["PiratesSantander"] = new BlackMarketData { FriendlyName = "Santander Pirates", PirateHavenID = "Santander(SantandersWorld)", BlackMarketName = "Santander", BlackMarketColor = "#c0c0c0" },
            ["PiratesTortuga"] = new BlackMarketData { FriendlyName = "Tortuga Pirates", PirateHavenID = "TortugaPrime", BlackMarketName = "Tortuga", BlackMarketColor = "#228b22" },
            ["PiratesValkyrate"] = new BlackMarketData { FriendlyName = "Valkyrate Pirates", PirateHavenID = "Gotterdammerung", BlackMarketName = "Coreward", BlackMarketColor = "#87851c" }
        };

        private static readonly Dictionary<string, string> PirateHavenToFaction;
        private static readonly HashSet<string> PirateHavenSystemIDs;
        private static readonly Dictionary<string, Color> FactionToColorMap;

        static BlackMarket()
        {
            PirateHavenToFaction = BlackMarketFactions
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value.PirateHavenID))
                .ToDictionary(kvp => $"starsystemdef_{kvp.Value.PirateHavenID}",
                              kvp => kvp.Key);

            PirateHavenSystemIDs = [.. PirateHavenToFaction.Keys];

            FactionToColorMap = [];
            foreach (var kvp in BlackMarketFactions)
            {
                if (ColorUtility.TryParseHtmlString(kvp.Value.BlackMarketColor, out var color))
                {
                    FactionToColorMap[kvp.Key] = color;
                }
            }
        }

        private SimGameState _simGame;
        private readonly float _dimLevel = dimLevel;
        private string _highlightedFactionID;

        #endregion

        #region IMapMode Implementation

        public string Name { get; } = "Black Market Zones";

        public void Apply(SimGameState simGame)
        {
            _simGame = simGame;
            _highlightedFactionID = null;
            _simGame.Starmap.StarSystemHovered.AddListener(OnSystemHovered);

            ApplyFilter();
            MapModesUI.BlackMarketFactionTextGameObject.SetActive(true);
            MapModesUI.NavigationScreen.ShowSpecialSystems();
        }

        public void Unapply(SimGameState simGame)
        {
            _simGame?.Starmap.StarSystemHovered.RemoveListener(OnSystemHovered);
            _simGame = null;
            _highlightedFactionID = null;

            foreach (var system in simGame.StarSystemDictionary.Values)
            {
                MapModesUI.DimSystem(system.ID, 1);
            }
            MapModesUI.NavigationScreen.ShowSpecialSystems();
        }

        #endregion

        #region Mode Logic

        private void OnSystemHovered(StarSystem hoveredSystem)
        {
            if (_simGame == null) return;

            string newFactionID = null;
            if (hoveredSystem != null)
            {
                newFactionID = hoveredSystem.Def.ContractTargetIDList.FirstOrDefault(FactionToColorMap.ContainsKey);
            }

            if (!string.IsNullOrEmpty(newFactionID) && newFactionID != _highlightedFactionID)
            {
                _highlightedFactionID = newFactionID;
                MapModesUI.BlackMarketFactionText.text = BlackMarketFactions.TryGetValue(newFactionID, out var data) ? data.FriendlyName : string.Empty;
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            if (_simGame == null) return;

            foreach (var system in _simGame.StarSystemDictionary.Values)
            {
                string controllingFaction = system.Def.ContractTargetIDList.FirstOrDefault(FactionToColorMap.ContainsKey);
                if (controllingFaction == null)
                {
                    MapModesUI.DimSystem(system.ID, _dimLevel);
                    continue;
                }

                var baseColor = FactionToColorMap[controllingFaction];
                float dimLevel;

                if (_highlightedFactionID == null)
                {
                    dimLevel = _dimLevel / 2f;
                }
                else
                {
                    bool isInHighlightedZone = system.Def.ContractTargetIDList.Contains(_highlightedFactionID);
                    dimLevel = isInHighlightedZone ? 1f : _dimLevel / 2f;
                }

                MapModesUI.ColorSystem(system.ID, baseColor / dimLevel);
            }
        }

        #endregion

        #region Data Initialization

        internal static void AddPirateHavenTags(DataManager dataManager)
        {
            foreach (var blackMarketData in BlackMarketFactions.Values)
            {
                if (string.IsNullOrEmpty(blackMarketData.PirateHavenID)) continue;

                string systemId = $"starsystemdef_{blackMarketData.PirateHavenID}";
                if (dataManager.SystemDefs.TryGet(systemId, out var systemDef))
                {
                    if (IsPirateFaction(blackMarketData))
                    {
                        if (!systemDef.Tags.Contains("planet_other_piratehaven"))
                        {
                            systemDef.Tags.Add("planet_other_piratehaven");
                        }
                    }
                    else
                    {
                        if (!systemDef.Tags.Contains("planet_other_criminalhub"))
                        {
                            systemDef.Tags.Add("planet_other_criminalhub");
                        }
                    }
                }
            }
        }

        #endregion

        #region UI Indicators

        internal static void ShowPirateHavenIndicators(List<string> specialSystems, string systemID, StarmapSystemRenderer systemRenderer)
        {
            var system = systemRenderer.system.System;

            if (IsPirateHaven(system))
            {
                systemRenderer.SetBlackMarket(true);

                if (specialSystems.Contains(systemID))
                {
                    specialSystems.Remove(systemID);
                }
            }
            else
            {
                systemRenderer.SetBlackMarket(false);
            }
        }

        internal static void ShowBlackMarketOwner(StarSystem system, GameObject indicatorObj, HBSTooltip indicatorToolTip)
        {
            string blackMarketFactionId = system.Def.ContractTargetIDList
                .FirstOrDefault(BlackMarketFactions.ContainsKey);

            if (!string.IsNullOrEmpty(blackMarketFactionId))
            {
                var blackMarketData = BlackMarketFactions[blackMarketFactionId];
                var label = indicatorObj.transform.Find("labelBox/label-text");
                label?.GetComponent<TMPro.TextMeshProUGUI>().text = blackMarketData.FriendlyName;

                if (indicatorToolTip != null)
                {
                    string tooltipText = $"You have access to the underground black market in this system, operated by the {blackMarketData.FriendlyName}.";
                    indicatorToolTip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(tooltipText));
                }
            }
        }

        #endregion

        #region Helpers

        public static bool IsPirateFaction(BlackMarketData data) => data.FriendlyName.Contains("Pirates");

        public static bool IsPirateHaven(StarSystem system) => system != null && PirateHavenSystemIDs.Contains(system.ID);

        #endregion

        #region Nested Types

        public struct BlackMarketData
        {
            public string FriendlyName;
            public string PirateHavenID;
            public string BlackMarketName;
            public string BlackMarketColor;
        }

        #endregion
    }
}