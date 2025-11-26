using BattleTech;
using NavigationComputer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NavigationComputer.Features.MapModes
{
    /// <summary>
    /// Flashpoint map mode: Highlights systems with active flashpoints.
    /// </summary>
    public class Flashpoint(float dimLevel = 5f) : IMapMode
    {
        #region Fields and Properties

        public readonly HashSet<string> FlashpointSystemIds = [];

        private readonly float _dimLevel = dimLevel;
        private static DateTime _lastDayUpdated = new(1999, 1, 1);
        private static string _cachedFlashpointText = "";
        private static bool _flashpointTrackerHiddenByUser;

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
                    _lastDayUpdated = simGame.CurrentDate;
                    _cachedFlashpointText = BuildFlashpointTrackerText(simGame);
                }

                if (!string.IsNullOrEmpty(_cachedFlashpointText))
                {
                    ShowFlashpointTracker(_cachedFlashpointText);
                }
            }
        }

        public void Unapply(SimGameState simGame) => _flashpointTrackerHiddenByUser = false;

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

        private static string BuildFlashpointTrackerText(SimGameState simGame)
        {
            var sb = new StringBuilder();
            int cooldown = simGame.inFlashpointCooldown ? simGame.flashpointCooldownDays : 0;
            if (cooldown > 0)
            {
                sb.AppendLine($"New flashpoint in: {cooldown} days\n");
            }

            var flashpointsToShow = simGame.AvailableFlashpoints
                .Where(fp => fp.Def?.Description != null)
                .OrderBy(fp => fp.Def.Description.Name)
                .ToList();

            if (!flashpointsToShow.Any())
            {
                return sb.ToString();
            }

            sb.AppendLine("<b>AVAILABLE FLASHPOINTS</b>");

            foreach (var flashpoint in flashpointsToShow)
            {
                string timer = "";
                int remainingTime = flashpoint.GetRemainingTime();
                if (remainingTime < 9999)
                {
                    timer = $" ({remainingTime}d left)";
                }

                sb.AppendLine($"■ {flashpoint.Def.Description.Name}{timer}");
            }

            return sb.ToString();
        }

        #endregion

        #region UI Logic

        private static void ShowFlashpointTracker(string text)
        {
            MapModesUI.FlashpointTrackerText.text = text;
            MapModesUI.FlashpointTrackerGameObject.SetActive(true);
            MapModesUI.FlashpointTrackerText.alpha = _flashpointTrackerHiddenByUser ? 0f : 1f;

            MapModesUI.FlashpointTrackerGameObject.GetComponent<RectTransform>().Apply(rt =>
            {
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector3(-50, -150, 0);
            });
        }

        internal static void OnFlashpointTrackerClicked()
        {
            _flashpointTrackerHiddenByUser = !_flashpointTrackerHiddenByUser;
            MapModesUI.FlashpointTrackerText.alpha = _flashpointTrackerHiddenByUser ? 0f : 1f;
        }

        #endregion
    }
}
