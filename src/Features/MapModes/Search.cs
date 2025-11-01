using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Search map mode: Dims star systems that do not match the search criteria.
    /// </summary>
    public class Search(float dimLevel = 10f) : IMapMode
    {
        private static readonly Dictionary<string, string> TagIdToFriendlyName = new()
        {
            {"planet_size_large", "high gravity planet"},
            {"planet_size_medium", "medium gravity planet"},
            {"planet_size_small", "low gravity planet"},

            {"planet_climate_arctic", "arctic world"},
            {"planet_climate_arid", "arid world"},
            {"planet_climate_desert", "desert world"},
            {"planet_climate_ice", "ice world"},
            {"planet_climate_lunar", "lunar world"},
            {"planet_climate_mars", "martian world"},
            {"planet_climate_moon", "barren world"},
            {"planet_climate_rocky", "rocky world"},
            {"planet_climate_terran", "terran world"},
            {"planet_climate_tropical", "tropical world"},
            {"planet_climate_water", "water world"},

            {"planet_other_alienvegetation", "alien vegetation"},
            {"planet_other_battlefield", "battlefield"},
            {"planet_other_blackmarket", "black market"},
            {"planet_other_boreholes", "geothermal boreholes"},
            {"planet_other_capital", "regional capital"},
            {"planet_other_comstar", "comstar presence"},
            {"planet_other_dragons", "native dragons"},
            {"planet_other_empty", "uninhabited"},
            {"planet_other_floatingworld", "dense cloud layer"},
            {"planet_other_fungus", "dominant fungus"},
            {"planet_other_gasgiant", "gas giant moon"},
            {"planet_other_hub", "travel hub"},
            {"planet_other_megacity", "megacity"},
            {"planet_other_megaforest", "planetwide forest"},
            {"planet_other_moon", "moons"},
            {"planet_other_mudflats", "planetwide mudflats"},
            {"planet_other_newcolony", "recently colonized"},
            {"planet_other_pirate", "pirate presence"},
            {"planet_other_plague", "plague quarantine"},
            {"planet_other_prison", "prison planet"},
            {"planet_other_ruins", "ruins"},
            {"planet_other_starleague", "former star league presence"},
            {"planet_other_stonedcaribou", "hallucinatory vegetation"},
            {"planet_other_storms", "planetwide storms"},
            {"planet_other_taintedair", "tainted atmosphere"},
            {"planet_other_volcanic", "extensive vulcanism"},

            {"planet_pop_large", "large population"},
            {"planet_pop_medium", "moderate population"},
            {"planet_pop_none", "token population"},
            {"planet_pop_small", "small population"},

            {"planet_civ_innersphere", "inner sphere-level civilization"},
            {"planet_civ_periphery", "periphery-level civilization"},
            {"planet_civ_primitive", "primitive civilization"},

            {"planet_industry_rich", "rich"},
            {"planet_industry_poor", "poor"},

            {"planet_industry_agriculture", "agriculture"},
            {"planet_industry_aquaculture", "aquaculture"},
            {"planet_industry_chemicals", "chemicals"},
            {"planet_industry_electronics", "electronics"},
            {"planet_industry_manufacturing", "manufacturing"},
            {"planet_industry_mining", "mining"},
            {"planet_industry_recreation", "recreation"},
            {"planet_industry_research", "research"},

            {"planet_faction_independent", "independent"}
        };

        private readonly float _dimLevel = dimLevel;

        public string Name { get; } = "System Search";

        public void Apply(SimGameState simGame)
        {
            MapModesUI.MapSearchGameObject.SetActive(true);
            MapModesUI.MapSearchInputField.onValueChanged.AddListener(x => ApplyFilter(simGame, x));
            MapModesUI.MapSearchInputField.ActivateInputField();
        }

        public void Unapply(SimGameState simGame)
        {
            MapModesUI.MapSearchInputField.onValueChanged.RemoveAllListeners();
            MapModesUI.MapSearchGameObject.SetActive(false);
        }

        private bool DoesFactionMatchSearch(string factionID, string search)
        {
            var def = FactionDef.GetFactionDefByEnum(UnityGameInstance.BattleTechGame.DataManager, factionID);
            string name = def.Name.StartsWith("the ", StringComparison.OrdinalIgnoreCase) ? def.Name.Substring(4).ToLower() : def.Name.ToLower();
            string shortName = def.ShortName.ToLower();

            return name.Contains(search) || shortName.Contains(search);
        }

        private bool DoesTagMatchSearch(string tagID, string search) =>
            (TagIdToFriendlyName.ContainsKey(tagID) && TagIdToFriendlyName[tagID].Contains(search)) ||
            (Main.Settings.SearchableTags.ContainsKey(tagID) && Main.Settings.SearchableTags[tagID].Contains(search));

        private bool DoesSystemMatchSearch(StarSystem system, SearchValue search)
        {
            if (string.IsNullOrEmpty(search.Value))
                return true;

            static bool ComStarSearch(StarSystem system, string searchString) => "comstar".Contains(searchString) && (system.Tags.Contains("planet_other_comstar") || system.Tags.Contains("planet_other_starleague"));

            var matches = search.Type switch
            {
                "name" => system.Name.ToLower().Contains(search.Value),
                "for" or "employer" => system.Def.ContractEmployerIDList.Any(faction => DoesFactionMatchSearch(faction, search.Value)),
                "against" or "target" => system.Def.ContractTargetIDList.Any(faction => DoesFactionMatchSearch(faction, search.Value)) || ComStarSearch(system, search.Value),
                "tag" => system.Tags.Any(tagID => DoesTagMatchSearch(tagID, search.Value)),
                "" => system.Name.ToLower().Contains(search.Value) ||
                                              system.Def.ContractEmployerIDList.Any(faction => DoesFactionMatchSearch(faction, search.Value)) ||
                                              system.Tags.Any(tagID => DoesTagMatchSearch(tagID, search.Value)),
                _ => false,
            };
            return search.Inverted ? !matches : matches;
        }

        private void ApplyFilter(SimGameState simGame, string searchString)
        {
            searchString = searchString.ToLower();
            string[] andSplit = searchString.Split('&');
            var searchTree = andSplit.Select(andTerm => andTerm.Split('|').Select(orTerm => new SearchValue(orTerm.Trim())).ToArray()).ToArray();

            foreach (string systemID in simGame.StarSystemDictionary.Keys)
            {
                var system = simGame.StarSystemDictionary[systemID];
                bool matches = searchTree.All(andTerm => andTerm.Any(searchValue => DoesSystemMatchSearch(system, searchValue)));
                MapModesUI.DimSystem(systemID, matches ? 1 : _dimLevel);
            }
        }

        private class SearchValue
        {
            private static readonly Regex ColonRegex = new(@"^((?<type>\w+):)?\s?(?<search>.+)$\s?");

            public string Value;
            public string Type;
            public bool Inverted;

            public SearchValue(string searchString)
            {
                searchString = searchString.Trim();

                if (searchString.StartsWith("-"))
                {
                    searchString = searchString.Remove(0, 1);
                    Inverted = true;
                }

                var regexMatch = ColonRegex.Match(searchString);
                Type = regexMatch.Groups["type"].Value;
                Value = regexMatch.Groups["search"].Value;
            }
        }
    }
}
