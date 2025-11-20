using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using NavigationComputer.Features.MapModes;
using NavigationComputer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NavigationComputer.Features.MapModesUI;

namespace NavigationComputer.Features
{
    /// <summary>
    /// Manages the indicator filter dropdown and filtering logic on the navigation screen.
    /// </summary>
    public static class IndicatorFilter
    {
        #region UI Elements

        private static GameObject _indicatorDropdown;
        private static GameObject _divisor;
        private static GameObject _difficultyDropdown;
        private static GameObject _biomeDropdown;
        private static GameObject _storeDropdown;

        #endregion

        #region Filter States

        public static bool ShowBlackMarkets { get; set; } = true;
        public static bool ShowFactories { get; set; } = true;
        public static bool ShowFlashpoints { get; set; } = true;
        public static bool AreFiltersVisible { get; private set; }
        private static bool? _visibilityBeforeMapMode;

        #endregion

        #region UI Setup

        /// <summary>
        /// Sets up the indicator filter dropdown on the navigation screen.
        /// </summary>
        internal static void SetupFilterDropdown(SGNavigationScreen navScreen)
        {
            try
            {
                var filterContainer = navScreen.DifficultyDropdown.transform.parent.gameObject;
                if (filterContainer == null) return;

                filterContainer.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleRight;
                filterContainer.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);

                var indicatorDropdown = UnityEngine.Object.Instantiate(navScreen.DifficultyDropdown.gameObject, filterContainer.transform, false);
                indicatorDropdown.name = "uixPrfDrop_IndicatorDropdown-MANAGED";
                indicatorDropdown.transform.SetAsFirstSibling();

                var dropdown = indicatorDropdown.GetComponentInChildren<HBS_Dropdown>(true);
                PopulateDropdown(navScreen, filterContainer, indicatorDropdown, dropdown);
                SanitizeDropdown(indicatorDropdown);
            }
            catch (Exception ex)
            {
                Main.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Populates the indicator filter dropdown with options and sets up event listeners.
        /// </summary>
        private static void PopulateDropdown(SGNavigationScreen navScreen, GameObject filterContainer, GameObject indicatorDropdown, HBS_Dropdown dropdown)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(["All", "No Black Markets", "No Factories", "None"]);
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(OnFilterChanged);

            indicatorDropdown.transform.Find("FilterDifficultyName").GetComponent<LocalizableText>().Apply(txt =>
            {
                txt.name = "FilterIndicatorName";
                txt.SetText("Indicator Filter");
            });

            var divisor = filterContainer.transform.Find("div").gameObject;
            divisor.transform.SetSiblingIndex(1);

            var infoIcon = navScreen.StoreDropdown.transform.Find("uixPrfIcon_BASE_infoIcon-MANAGED").gameObject;
            if (infoIcon != null)
            {
                infoIcon.transform.SetParent(filterContainer.transform, false);
                infoIcon.transform.SetAsLastSibling();

                _indicatorDropdown = indicatorDropdown;
                _divisor = divisor;
                _difficultyDropdown = navScreen.DifficultyDropdown.gameObject;
                _biomeDropdown = navScreen.BiomeDropdown.gameObject;
                _storeDropdown = navScreen.StoreDropdown.gameObject;

                SetFilterVisibility(false);

                infoIcon.GetComponent<HBSDOTweenToggle>().Apply(btn =>
                {
                    btn.OnClicked.RemoveAllListeners();
                    btn.OnClicked.AddListener(() => SetFilterVisibility(!_indicatorDropdown.activeSelf));
                });
            }
        }

        /// <summary>
        /// Sanitizes the indicator filter dropdown by removing unnecessary elements.
        /// </summary>
        private static void SanitizeDropdown(GameObject indicatorDropdown)
        {
            UnityEngine.Object.Destroy(indicatorDropdown.transform.Find("selectedItemImage (1)").gameObject);

            foreach (var transform in indicatorDropdown.GetComponentsInChildren<RectTransform>(true))
            {
                if (transform.name == "uixPrfIndc_SIM_skullDifficultyWidget-MANAGED")
                    UnityEngine.Object.Destroy(transform.gameObject);

                else if (transform.name == "Item Label")
                    transform.gameObject.SetActive(true);
            }
        }

        #endregion

        #region Dropdown Logic

        /// <summary>
        /// Handles changes to the indicator filter dropdown.
        /// </summary>
        public static void OnFilterChanged(int index)
        {
            ShowBlackMarkets = index is not 1 and not 3;
            ShowFactories = index is not 2 and not 3;
            ShowFlashpoints = index is not 3;

            if (CurrentMapMode == null)
                NavigationScreen?.RefreshSystemIndicators();
        }

        /// <summary>
        /// Sets the visibility of the various filter dropdowns.
        /// </summary>
        public static void SetFilterVisibility(bool show)
        {
            _indicatorDropdown?.SetActive(show);
            _divisor?.SetActive(show);
            _difficultyDropdown?.SetActive(show);
            _biomeDropdown?.SetActive(show);
            _storeDropdown?.SetActive(show);
            AreFiltersVisible = show;
        }

        /// <summary>
        /// Saves filter visibility state and hides them when a map mode is turned on.
        /// </summary>
        public static void HideForMapMode()
        {
            _visibilityBeforeMapMode ??= AreFiltersVisible;
            SetFilterVisibility(false);
        }

        /// <summary>
        /// Restores filter visibility to its previous state when a map mode is turned off.
        /// </summary>
        public static void RestoreAfterMapMode()
        {
            if (_visibilityBeforeMapMode.HasValue)
            {
                SetFilterVisibility(_visibilityBeforeMapMode.Value);
                _visibilityBeforeMapMode = null;
            }
        }

        #endregion

        #region Indicator Logic

        public static bool ShouldShowFactionStore(SimGameState simGame, StarSystem system, FactionValue faction) =>
            simGame.IsSystemFactionStore(system, faction) && (CurrentMapMode is Factory || IsFactionAlly(simGame, faction, null) || ShowBlackMarkets);

        public static bool ShouldShowBlackMarketIndicator(SimGameState simGame, StarSystem system) =>
            (CurrentMapMode is BlackMarket && BlackMarket.IsPirateHaven(system)) || (CurrentMapMode is null && ShowBlackMarkets && simGame.IsSystemBlackMarket(system));

        public static bool ShouldShowFactoryIndicator(SimGameState simGame, FactionValue faction, List<string> allyListOverride) =>
            CurrentMapMode is Factory || (CurrentMapMode is null && ShowFactories && IsFactionAlly(simGame, faction, allyListOverride));

        private static bool IsFactionAlly(SimGameState simGame, FactionValue faction, List<String> allyListOverride) =>
            Main.BTFactionStoreUnlockDetected || simGame.IsFactionAlly(faction, allyListOverride);

        #endregion
    }
}