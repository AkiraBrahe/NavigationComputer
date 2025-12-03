using BattleTech;
using BattleTech.UI;
using NavigationComputer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static BattleTech.Flashpoint;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Flashpoint map mode: Highlights systems with active flashpoints.
    /// </summary>
    public class Flashpoint(float dimLevel = 5f) : IMapMode
    {
        #region Fields and Properties

        public static readonly Dictionary<string, FlashpointData> TimedFlashpoints = new()
        {
            ["Birth Of A Legend"] = new FlashpointData { StarSystem = "Trell", StartDate = DateTime.MinValue, EndDate = new DateTime(3028, 1, 1), PrereqFlashpoint = null, CampaignName = "Gray Death Legion", CampaignOrder = "1/5" },
            ["Joint Venture"] = new FlashpointData { StarSystem = "Addicks", StartDate = DateTime.MinValue, EndDate = new DateTime(3028, 1, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["Prototype"] = new FlashpointData { StarSystem = "Fagerholm", StartDate = DateTime.MinValue, EndDate = new DateTime(3028, 1, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["The Baying of Hounds"] = new FlashpointData { StarSystem = "Viribium", StartDate = DateTime.MinValue, EndDate = new DateTime(3030, 1, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["Betrayal At Helm"] = new FlashpointData { StarSystem = "Helm", StartDate = new DateTime(3028, 1, 1), EndDate = new DateTime(3031, 1, 1), PrereqFlashpoint = null, CampaignName = "Gray Death Legion", CampaignOrder = "2/5" },
            ["Smash and Grab"] = new FlashpointData { StarSystem = "Repulse", StartDate = new DateTime(3030, 9, 1), EndDate = new DateTime(3031, 8, 27), PrereqFlashpoint = null, CampaignName = "Andurien Crisis", CampaignOrder = "1/2" },
            ["The Meatgrinder"] = new FlashpointData { StarSystem = "Betelgeuse", StartDate = new DateTime(3031, 2, 15), EndDate = new DateTime(3032, 5, 23), PrereqFlashpoint = "Smash and Grab", CampaignName = "Andurien Crisis", CampaignOrder = "2/2" },
            ["The Opportunist"] = new FlashpointData { StarSystem = "Awano", StartDate = new DateTime(3034, 4, 1), EndDate = new DateTime(3056, 4, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = "TBD Start" },
            ["Requiem for Ronin"] = new FlashpointData { StarSystem = "Al Hillah", StartDate = new DateTime(3035, 1, 1), EndDate = new DateTime(3039, 1, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["Fighting Ghosts"] = new FlashpointData { StarSystem = "Altais", StartDate = new DateTime(3039, 1, 1), EndDate = new DateTime(3042, 1, 1), PrereqFlashpoint = null, CampaignName = "Gray Death Legion", CampaignOrder = "3/5" },
            ["War of '39"] = new FlashpointData { StarSystem = "Setubal", StartDate = new DateTime(3039, 4, 16), EndDate = new DateTime(3039, 12, 30), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["LosTech Fever"] = new FlashpointData { StarSystem = "Baliggora", StartDate = new DateTime(3041, 1, 1), EndDate = new DateTime(3047, 1, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["Hidden Agendas"] = new FlashpointData { StarSystem = "Astrokaszy", StartDate = new DateTime(3043, 1, 1), EndDate = new DateTime(3048, 1, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["Falcon's Fury"] = new FlashpointData { StarSystem = "Sudeten", StartDate = new DateTime(3050, 1, 1), EndDate = new DateTime(3052, 1, 1), PrereqFlashpoint = null, CampaignName = "Gray Death Legion", CampaignOrder = "4/5" },
            ["Second Try"] = new FlashpointData { StarSystem = "Pandora", StartDate = DateTime.MinValue, EndDate = new DateTime(3053, 1, 1), PrereqFlashpoint = "Falcon's Fury", CampaignName = "Gray Death Legion", CampaignOrder = "5/5" },
            ["Battle of Tukayyid"] = new FlashpointData { StarSystem = "Tukayyid", StartDate = new DateTime(3052, 1, 1), EndDate = new DateTime(3053, 1, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },
            ["Insurgency"] = new FlashpointData { StarSystem = "Eaton", StartDate = new DateTime(3055, 8, 1), EndDate = new DateTime(3056, 12, 1), PrereqFlashpoint = null, CampaignName = null, CampaignOrder = null },

            ["Spite & Violence"] = new FlashpointData { StarSystem = "Cadiz (DC)", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = null, CampaignName = "Spite & Violence", CampaignOrder = "1/5" },
            ["Special Offer: Armed Robbery"] = new FlashpointData { StarSystem = "Kitalpha", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Spite & Violence", CampaignName = "Spite & Violence", CampaignOrder = "3/5" },
            ["Special Offer: Urbie's Got A Gun"] = new FlashpointData { StarSystem = "Rukbat", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Spite & Violence", CampaignName = "Spite & Violence", CampaignOrder = "2/5" },
            ["Spite & Violence: Rat Race"] = new FlashpointData { StarSystem = "Blue Diamond", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Spite & Violence", CampaignName = "Spite & Violence", CampaignOrder = "4/5" },
            ["Special Offer: Fistful Of Diamonds"] = new FlashpointData { StarSystem = "Porrima", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Spite & Violence: Rat Race", CampaignName = "Spite & Violence", CampaignOrder = "5/5" },
            ["Of Unknown Origin"] = new FlashpointData { StarSystem = "Tarragona", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = null, CampaignName = "Heavy Metal", CampaignOrder = "1/4" },
            ["Hunting Season"] = new FlashpointData { StarSystem = "Independence", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Of Unknown Origin", CampaignName = "Heavy Metal", CampaignOrder = "2/4" },
            ["Hourglass"] = new FlashpointData { StarSystem = "Appian", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Hunting Season", CampaignName = "Heavy Metal", CampaignOrder = "3/4" },
            ["Standoff"] = new FlashpointData { StarSystem = "Mantharaka", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Hourglass", CampaignName = "Heavy Metal", CampaignOrder = "4/4" },
            ["Under The Sun"] = new FlashpointData { StarSystem = "Raman", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Of Unknown Origin", CampaignName = "Old Friend", CampaignOrder = "1/3" },
            ["Old Walls & New Friends"] = new FlashpointData { StarSystem = "Greenlaw", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Under The Sun", CampaignName = "Old Friend", CampaignOrder = "2/3" },
            ["Better Left Buried"] = new FlashpointData { StarSystem = "Nightwish", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Old Walls & New Friends", CampaignName = "Old Friend", CampaignOrder = "3/3" },
            ["Mechs, Mercs & Rock'n'Roll"] = new FlashpointData { StarSystem = "Notwina", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "The Opportunist", CampaignName = "The Big Deal", CampaignOrder = "1/5" },
            ["Wild Wedding"] = new FlashpointData { StarSystem = "New Avalon", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Mechs, Mercs & Rock'n'Roll", CampaignName = "The Big Deal", CampaignOrder = "2/5" },
            ["Black Sabbath"] = new FlashpointData { StarSystem = "Helland", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Wild Wedding", CampaignName = "The Big Deal", CampaignOrder = "3/5" },
            ["Run Through The Jungle"] = new FlashpointData { StarSystem = "Choudrant", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Black Sabbath", CampaignName = "The Big Deal", CampaignOrder = "4/5" },
            ["The Big Deal"] = new FlashpointData { StarSystem = "Bryant", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, PrereqFlashpoint = "Run Through The Jungle", CampaignName = "The Big Deal", CampaignOrder = "5/5" }
        };

        public readonly HashSet<string> FlashpointSystemIds = [];

        private readonly float _dimLevel = dimLevel;
        internal static DateTime _lastDayUpdated = new(1999, 1, 1);
        private static string _cachedBasicFlashpointText = "";
        private static string _cachedAdvancedFlashpointText = "";
        private static TrackerState _flashpointTrackerState = TrackerState.Basic;

        #endregion

        #region IMapMode Implementation

        public string Name { get; } = "Active Flashpoints";

        public void Apply(SimGameState simGame)
        {
            FlashpointSystemIds.Clear();

            foreach (var flashpoint in simGame.AvailableFlashpoints)
            {
                if (flashpoint.CurSystem != null)
                {
                    FlashpointSystemIds.Add(flashpoint.CurSystem.ID);
                }
            }

            HighlightFlashpointSystems(simGame);

            if (Main.Settings.MapModes.ShowFlashpointTracker)
            {
                if (simGame.CurrentDate > _lastDayUpdated)
                {
                    UpdateFlashpointStatus(simGame);

                    _lastDayUpdated = simGame.CurrentDate;
                    _cachedBasicFlashpointText = BuildBasicFlashpointTrackerText(simGame);
                    _cachedAdvancedFlashpointText = BuildAdvancedFlashpointTrackerText();
                }

                UpdateTrackerDisplay();
            }
        }

        public void Unapply(SimGameState simGame) => _flashpointTrackerState = TrackerState.Basic;

        #endregion

        #region Mode Logic

        private void HighlightFlashpointSystems(SimGameState simGame)
        {
            var allSystems = simGame.StarSystemDictionary.Values.ToList();
            foreach (var system in allSystems)
            {
                if (!FlashpointSystemIds.Contains(system.ID))
                {
                    MapModesUI.DimSystem(system.ID, _dimLevel);
                }
            }
        }

        private static void UpdateFlashpointStatus(SimGameState simGame)
        {
            var flashpointKeys = TimedFlashpoints.Keys.ToList();

            foreach (var key in flashpointKeys)
            {
                if (TimedFlashpoints.TryGetValue(key, out var data))
                {
                    Status? newStatus;

                    if (simGame.completedFlashpoints.Contains(key))
                    {
                        newStatus = Status.COMPLETE_SUCCESS;
                    }
                    else if (simGame.CurrentDate > data.EndDate)
                    {
                        newStatus = Status.TIMED_OUT;
                    }
                    else if (simGame.CurrentDate >= data.StartDate &&
                        (string.IsNullOrEmpty(data.PrereqFlashpoint) || simGame.completedFlashpoints.Contains(data.PrereqFlashpoint)))
                    {
                        newStatus = Status.AVAILABLE;
                    }
                    else
                    {
                        newStatus = null;
                    }

                    if (data.Status != newStatus)
                    {
                        data.Status = newStatus;
                        TimedFlashpoints[key] = data;
                    }
                }
            }
        }

        private static string BuildBasicFlashpointTrackerText(SimGameState simGame)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>ACTIVE FLASHPOINTS</b>");

            var flashpointsToShow = simGame.AvailableFlashpoints
                .Where(fp => fp.Def?.Description != null)
                .OrderBy(fp => fp.Def.Description.Name)
                .ToList();

            if (!flashpointsToShow.Any())
            {
                sb.AppendLine("No flashpoints available.");
                return sb.ToString();
            }

            foreach (var flashpoint in flashpointsToShow)
            {
                string timer = "";
                int remainingTime = flashpoint.GetRemainingTime();
                if (remainingTime < 9999)
                {
                    timer = $" ({remainingTime}d left)";
                }

                var flashpointName = flashpoint.Def.Description.Name;
                if (flashpointName.StartsWith("Special Offer: ")) flashpointName.Substring(15);

                sb.AppendLine(RichTextWrapper.WrapLine($"[ ] {flashpointName}{timer}"));
            }

            return sb.ToString();
        }

        private static string BuildAdvancedFlashpointTrackerText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>TIMED FLASHPOINTS</b>");

            static DateTime GetEffectiveStartDate(KeyValuePair<string, FlashpointData> kvp) => kvp.Value.StartDate != DateTime.MinValue
                    ? kvp.Value.StartDate
                    : !string.IsNullOrEmpty(kvp.Value.PrereqFlashpoint) && TimedFlashpoints.TryGetValue(kvp.Value.PrereqFlashpoint, out var prereqData)
                        ? prereqData.StartDate
                        : DateTime.MinValue;

            var timedFlashpoints = TimedFlashpoints
                .Where(kvp => kvp.Value.EndDate != DateTime.MaxValue &&
                              !(kvp.Key == "The Opportunist" && !Main.HasTBD))
                .Select(kvp => new FlashpointSortData
                {
                    Name = kvp.Key,
                    Data = kvp.Value,
                    EffectiveStartDate = GetEffectiveStartDate(kvp)
                })
                .GroupBy(fp => fp.EffectiveStartDate.Year)
                .OrderBy(g => g.Key);

            if (!timedFlashpoints.Any())
            {
                sb.AppendLine("No timed flashpoints to track.");
                return sb.ToString();
            }

            foreach (var yearGroup in timedFlashpoints)
            {
                bool isGroupCompleted = yearGroup.All(fp => fp.Data.Status is Status.COMPLETE_SUCCESS or Status.TIMED_OUT);
                string headerColorStart = isGroupCompleted ? "<color=#555555>" : "";
                string headerColorEnd = isGroupCompleted ? "</color>" : "";

                IOrderedEnumerable<FlashpointSortData> sortedFlashpoints;
                if (yearGroup.Key <= 1)
                {
                    sb.AppendLine($"\n\u00A0{headerColorStart}<b>3025+:</b>{headerColorEnd}");
                    sortedFlashpoints = yearGroup.OrderBy(fp => fp.Data.EndDate);
                }
                else
                {
                    sb.AppendLine($"\n\u00A0{headerColorStart}<b>{yearGroup.Key}:</b>{headerColorEnd}");
                    sortedFlashpoints = yearGroup.OrderBy(fp => fp.EffectiveStartDate);
                }

                foreach (var flashpoint in sortedFlashpoints)
                {
                    string name = flashpoint.Name;
                    var data = flashpoint.Data;

                    var status = data.Status switch
                    {
                        Status.COMPLETE_SUCCESS => "<color=#555555>[<mspace=1em>✓</mspace>]",
                        Status.TIMED_OUT => "<color=#555555>[<mspace=1em>X</mspace>]",
                        Status.AVAILABLE => "<color=#F79B26>[<mspace=1em> </mspace>]",
                        _ => "<color=#FFFFFF>[ ]"
                    };

                    string suffix = "";
                    if (!string.IsNullOrEmpty(data.CampaignOrder))
                    {
                        suffix = $" ({data.CampaignOrder}";
                        if (data.CampaignName == "Gray Death Legion")
                        {
                            suffix += " GDL";
                        }
                        suffix += ")";
                    }

                    sb.AppendLine($"{status} {name} - {data.StarSystem}{suffix}</color>");

                    if (data.Status is Status.AVAILABLE)
                    {
                        sb.AppendLine($"\t<color=#F79B26>Available until {data.EndDate:MMMM yyyy}</color>");
                    }
                }
            }

            return sb.ToString();
        }

        #endregion

        #region UI Logic

        private static void ShowFlashpointTracker(string text)
        {
            MapModesUI.FlashpointTrackerText.text = text;
            MapModesUI.FlashpointTrackerGameObject.SetActive(true);
            MapModesUI.FlashpointTrackerText.alpha = _flashpointTrackerState == TrackerState.Hidden ? 0f : 1f;

            MapModesUI.FlashpointTrackerGameObject.GetComponent<RectTransform>().Apply(rt =>
            {
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector3(-50, -150, 0);
            });
        }

        private static void UpdateTrackerDisplay()
        {
            switch (_flashpointTrackerState)
            {
                case TrackerState.Basic:
                    ShowFlashpointTracker(_cachedBasicFlashpointText);
                    break;
                case TrackerState.Advanced:
                    ShowFlashpointTracker(_cachedAdvancedFlashpointText);
                    break;
                case TrackerState.Hidden:
                    ShowFlashpointTracker(_cachedBasicFlashpointText);
                    break;
            }
        }

        internal static void OnFlashpointTrackerClicked()
        {
            _flashpointTrackerState = _flashpointTrackerState switch
            {
                TrackerState.Basic => TrackerState.Advanced,
                TrackerState.Advanced => TrackerState.Hidden,
                TrackerState.Hidden => TrackerState.Basic,
                _ => TrackerState.Basic
            };

            UpdateTrackerDisplay();
        }

        #endregion

        #region Nested Types

        public struct FlashpointData
        {
            public string StarSystem;
            public DateTime StartDate;
            public DateTime EndDate;
            public string PrereqFlashpoint;
            public string CampaignName;
            public string CampaignOrder;
            public Status? Status;
        }

        public enum TrackerState
        {
            Hidden,
            Basic,
            Advanced
        }

        private struct FlashpointSortData
        {
            public string Name;
            public FlashpointData Data;
            public DateTime EffectiveStartDate;
        }

        #endregion
    }
}