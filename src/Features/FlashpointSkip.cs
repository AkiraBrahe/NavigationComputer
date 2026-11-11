using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using HBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NavigationComputer.Features
{
    #region Flashpoint Skip Logic

    /// <summary>
    /// Manages the logic for skipping flashpoints in the game.
    /// </summary>
    public static class FlashpointSkip
    {
        private static Dictionary<string, DateTime> SkippedFlashpoints => Main.CampaignState.SkippedFlashpoints;

        #region Backend Logic

        /// <summary>
        /// Skips the specified flashpoint, marking it as unavailable for a duration defined in the settings.
        /// </summary>
        public static bool SkipFlashpoint(Flashpoint flashpoint, SimGameState simGame)
        {
            if (!CanSkipFlashpoint(flashpoint, simGame))
                return false;

            string flashpointId = flashpoint.Def.Description.Id;
            SkippedFlashpoints[flashpointId] = simGame.CurrentDate;

            simGame.CompleteFlashpoint(flashpoint, FlashpointEndType.UserAborted);
            simGame.inFlashpointCooldown = false;
            flashpoint.SetStatus(Flashpoint.Status.REMOVED);
            MapModesUI.NavigationScreen?.RefreshSystemIndicators();
            return true;
        }

        /// <summary>
        /// Determines if the flashpoint can be skipped.
        /// </summary>
        public static bool CanSkipFlashpoint(Flashpoint flashpoint, SimGameState simGame)
        {
            return flashpoint?.Def?.Description != null
                && !IsTimedFlashpoint(flashpoint)
                && simGame != null
                && flashpoint != simGame.ActiveFlashpoint
                && simGame.AvailableFlashpoints.Contains(flashpoint);
        }

        /// <summary>
        /// Determines if the flashpoint is a time-limited flashpoint.
        /// </summary>
        public static bool IsTimedFlashpoint(Flashpoint flashpoint)
        {
            return flashpoint?.Def?.Description != null
                && MapModes.Flashpoint.FlashpointsByName.TryGetValue(flashpoint.Def.Description.Name, out var data)
                && data.EndDate != DateTime.MaxValue;
        }

        /// <summary>
        /// Filters out flashpoints that have been skipped from a list of candidates.
        /// </summary>
        public static List<FlashpointDef> FilterFlashpointCandidates(List<FlashpointDef> candidates, SimGameState simGame)
        {
            RemoveExpiredSkips(simGame.CurrentDate);
            return [.. candidates.Where(candidate => !SkippedFlashpoints.ContainsKey(candidate.Description.Id))];
        }

        #endregion

        #region Expiration Logic

        /// <summary>
        /// Removes expired skipped flashpoints from the list.
        /// </summary>
        private static void RemoveExpiredSkips(DateTime currentDate)
        {
            int durationMonths = Main.Settings.Gameplay.SkipFlashpointDurationMonths;
            if (durationMonths <= 0)
                return;

            foreach (string flashpointId in SkippedFlashpoints
                .Where(entry => currentDate >= entry.Value.AddMonths(durationMonths))
                .Select(entry => entry.Key)
                .ToList())
            {
                SkippedFlashpoints.Remove(flashpointId);
            }
        }

        /// <summary>
        /// Applies skip expiration policy based on the mod settings.
        /// </summary>
        public static void ApplySkipExpirationPolicy(SimGameState simGame)
        {
            int durationMonths = Main.Settings.Gameplay.SkipFlashpointDurationMonths;
            if (durationMonths == 0)
                SkippedFlashpoints.Clear();
            else
                RemoveExpiredSkips(simGame.CurrentDate);
        }

        #endregion

        #region UI Logic

        /// <summary>
        /// Increases the maximum number of active flashpoints based on the mod settings.
        /// </summary>
        public static void IncreaseMaximumFlashpointCount()
        {
            var constants = SimGameConstants.GetInstance(LazySingletonBehavior<UnityGameInstance>.Instance.Game);
            if (constants != null && constants.Flashpoints.MaxActiveFlashpoints < Main.Settings.Gameplay.MaxActiveFlashpoints)
            {
                constants.Flashpoints.MaxActiveFlashpoints = Main.Settings.Gameplay.MaxActiveFlashpoints;
                Main.Log.LogDebug($"Increased max number of active flashpoints to {Main.Settings.Gameplay.MaxActiveFlashpoints}.");
            }
        }

        /// <summary>
        /// Updates the tooltip for the flashpoint skip button.
        /// </summary>
        public static void UpdateSkipTooltip(SG_FlashpointInfoPopup popup)
        {
            var confirmButton = popup.ConfirmButton;
            if (confirmButton == null) return;

            var tooltip = confirmButton.GetComponent<HBSTooltip>()
                ?? confirmButton.gameObject.AddComponent<HBSTooltip>();
            var tooltipData = new HBSTooltipStateData();
            var flashpoint = FlashpointSkipButton.GetFlashpoint(popup);
            string tooltipText = IsTimedFlashpoint(flashpoint)
                ? "Timed flashpoints cannot be skipped."
                : GetSkipTooltipText();
            tooltipData.SetString(tooltipText);
            tooltip.SetDefaultStateData(tooltipData);
        }

        /// <summary>
        /// Generates the tooltip text for the flashpoint skip button.
        /// </summary>
        private static string GetSkipTooltipText()
        {
            int durationMonths = Main.Settings.Gameplay.SkipFlashpointDurationMonths;
            string durationText = durationMonths switch
            {
                <= -1 => "Skipped flashpoints will not return.",
                0 => "Skipped flashpoints will remain unavailable until you restart the game.",
                1 => "Skipped flashpoints will return after 1 month.",
                _ => $"Skipped flashpoints will return after {durationMonths} months."
            };

            return $"<b>Ctrl+Click</b> to skip this flashpoint. {durationText}";
        }

        #endregion
    }

    #endregion

    #region Flashpoint Skip Button

    /// <summary>
    /// Manages the display of the flashpoint skip button in the flashpoint info popup.
    /// </summary>
    public sealed class FlashpointSkipButton : MonoBehaviour
    {
        private static readonly FieldInfo FlashpointField = AccessTools.Field(typeof(SG_FlashpointInfoPopup), "theFlashpoint");
        private static readonly FieldInfo SimGameField = AccessTools.Field(typeof(SG_FlashpointInfoPopup), "simState");
        private static readonly FieldInfo ConfirmButtonField = AccessTools.Field(typeof(SG_FlashpointInfoPopup), "ConfirmButton");

        private SG_FlashpointInfoPopup popup;
        private GameObject disabledOverlay;
        private bool showingSkip;

        internal static bool ControlHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        internal static Flashpoint GetFlashpoint(SG_FlashpointInfoPopup popup) =>
            FlashpointField.GetValue(popup) as Flashpoint;

        internal static SimGameState GetSimGame(SG_FlashpointInfoPopup popup) =>
            SimGameField.GetValue(popup) as SimGameState;

        internal void Initialize(SG_FlashpointInfoPopup popup)
        {
            this.popup = popup;
            showingSkip = false;
            SetDisabledOverlay(false);
        }

        private void Update()
        {
            if (popup == null || !popup.Visible)
                return;

            var flashpoint = GetFlashpoint(popup);
            var simGame = GetSimGame(popup);
            var confirmButton = ConfirmButtonField.GetValue(popup) as HBSDOTweenButton;
            if (flashpoint == null || simGame == null || confirmButton == null)
                return;

            bool isAvailable = simGame.AvailableFlashpoints.Contains(flashpoint)
                && flashpoint != simGame.ActiveFlashpoint;
            if (ControlHeld && isAvailable)
            {
                confirmButton.SetText("Skip");
                SetDisabledOverlay(FlashpointSkip.IsTimedFlashpoint(flashpoint));
                showingSkip = true;
            }
            else if (showingSkip)
            {
                SetDisabledOverlay(false);
                showingSkip = false;
                popup.Refresh();
            }
        }

        private void SetDisabledOverlay(bool active)
        {
            if (disabledOverlay == null && popup != null)
            {
                var confirmButton = ConfirmButtonField.GetValue(popup) as HBSDOTweenButton;
                if (confirmButton != null)
                {
                    foreach (var child in confirmButton.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name == "disabledOverlay")
                        {
                            disabledOverlay = child.gameObject;
                            break;
                        }
                    }
                }
            }

            disabledOverlay?.SetActive(active);
        }
    }

    #endregion
}