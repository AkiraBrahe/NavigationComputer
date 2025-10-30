using BattleTech;
using BattleTech.UI;
using NavigationComputer.Features.MapModes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationComputer.Features
{
    /// <summary>
    /// Handles the UI and logic for map modes on the star map.
    /// </summary>
    public static class MapModesUI
    {
        internal static IMapMode CurrentMapMode;
        internal static readonly Dictionary<KeyCode, IMapMode> DiscreteMapModes = [];
        internal static IMapMode SearchMapMode;

        internal static SimGameState SimGame;
        internal static SGNavigationScreen NavigationScreen;

        private static readonly MaterialPropertyBlock MPB = new();
        private static float? _oldTravelIntensity;

        internal static GameObject MapModeTextGameObject;
        internal static TextMeshProUGUI MapModeText;
        internal static GameObject MapSearchGameObject;
        internal static TMP_InputField MapSearchInputField;

        #region Initialization

        /// <summary>
        /// Initializes the map modes and their corresponding key bindings.
        /// </summary>
        public static void Setup()
        {
            DiscreteMapModes.Add(KeyCode.F1, new Unvisited());
            DiscreteMapModes.Add(KeyCode.F2, new Difficulty());
            DiscreteMapModes.Add(KeyCode.F3, new Factory());
            SearchMapMode = new Search();
        }

        /// <summary>
        /// Sets up the necessary UI objects on the navigation screen.
        /// </summary>
        internal static void SetupUIObjects(SGNavigationScreen navScreen)
        {
            NavigationScreen = navScreen;

            if (MapModeTextGameObject == null)
            {
                MapModeTextGameObject = new GameObject("NavigationComputer-Text");
                MapModeTextGameObject.AddComponent<RectTransform>();
                MapModeText = MapModeTextGameObject.AddComponent<TextMeshProUGUI>();
                MapModeText.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
                MapModeText.alignment = TextAlignmentOptions.Center;
            }

            if (MapSearchGameObject == null)
            {
                MapSearchGameObject = new GameObject("NavigationComputer-Search");
                MapSearchGameObject.AddComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
                MapSearchInputField = MapSearchGameObject.AddComponent<TMP_InputField>();

                var textArea = new GameObject("NavigationComputer-Search-TextArea");
                textArea.AddComponent<RectMask2D>();
                textArea.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
                textArea.transform.SetParent(MapSearchGameObject.transform);

                var text = new GameObject("NavigationComputer-Search-Text");
                text.AddComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
                var textTMP = text.AddComponent<TextMeshProUGUI>();
                text.transform.SetParent(textArea.transform);

                textTMP.SetText(string.Empty);
                textTMP.enableWordWrapping = false;
                textTMP.extraPadding = true;
                textTMP.alignment = TextAlignmentOptions.Center;
                textTMP.fontSize *= 0.75f;

                MapSearchInputField.textComponent = textTMP;
                MapSearchInputField.textViewport = textArea.GetComponent<RectTransform>();
            }

            MapSearchGameObject.transform.SetParent(navScreen.transform);
            MapModeTextGameObject.transform.SetParent(navScreen.transform);

            // Setting TMP_FontAsset via Resources.Load doesn't work here for some reason
            var fonts = Resources.FindObjectsOfTypeAll(typeof(TMP_FontAsset));
            foreach (var o in fonts)
            {
                var font = (TMP_FontAsset)o;

                if (font.name == "UnitedSansReg-Black SDF")
                    MapModeText.font = font;

                if (font.name == "UnitedSansReg-Medium SDF")
                    MapSearchInputField.textComponent.font = font;
            }

            ResetMapUI();
        }

        #endregion

        #region Map Mode Management

        /// <summary>
        /// Toggles the current map mode based on the pressed key.
        /// </summary>
        internal static void ToggleMapMode(KeyCode key)
        {
            if (!SimGame.Starmap.Screen.StarmapVisible)
                return;

            if (DiscreteMapModes[key] == CurrentMapMode)
                TurnMapModeOff();
            else
                TurnMapModeOn(DiscreteMapModes[key]);
        }

        /// <summary>
        /// Starts the search map mode.
        /// </summary>
        internal static void StartSearching()
        {
            if (!SimGame.Starmap.Screen.StarmapVisible)
                return;

            if (CurrentMapMode != SearchMapMode)
            {
                TurnMapModeOff();
            }
            else
            {
                MapSearchInputField.DeactivateInputField();
                MapSearchInputField.ActivateInputField();
                MapSearchInputField.Select();
            }

            if (CurrentMapMode == null)
                TurnMapModeOn(SearchMapMode);
        }

        /// <summary>
        /// Turns on the current map mode.
        /// </summary>
        internal static void TurnMapModeOn(IMapMode mapMode)
        {
            if (CurrentMapMode != null)
                TurnMapModeOff();

            CurrentMapMode = mapMode;
            Main.Log.LogDebug($"Turning on map mode \"{CurrentMapMode.Name}\"");
            CurrentMapMode.Apply(SimGame);

            SetMapModeText(CurrentMapMode.Name);
            SetMapStuffActive(false);

            if (CurrentMapMode is Factory)
                NavigationScreen.ShowSpecialSystems();
        }

        /// <summary>
        /// Turns off the current map mode.
        /// </summary>
        internal static void TurnMapModeOff()
        {
            ResetMapUI();

            if (CurrentMapMode == null)
                return;

            Main.Log.LogDebug($"Turning off map mode \"{CurrentMapMode.Name}\"");

            CurrentMapMode.Unapply(SimGame);
            CurrentMapMode = null;

            SetMapStuffActive(true);
            NavigationScreen.RefreshWidget();
        }

        #endregion

        #region UI Management

        /// <summary>
        /// Sets the map mode text UI element.
        /// </summary>
        private static void SetMapModeText(string text)
        {
            MapModeText.text = text;
            MapModeTextGameObject.SetActive(true);

            var textRectTransform = MapModeTextGameObject.GetComponent<RectTransform>();
            var searchRectTransform = MapSearchGameObject.GetComponent<RectTransform>();

            textRectTransform.anchorMin = new Vector2(0.5f, 1);
            textRectTransform.anchorMax = new Vector2(0.5f, 1);
            textRectTransform.anchoredPosition = new Vector3(0, -75, 0);

            searchRectTransform.anchorMin = new Vector2(0.5f, 1);
            searchRectTransform.anchorMax = new Vector2(0.5f, 1);
            searchRectTransform.anchoredPosition = new Vector3(0, -115, 0);
        }

        /// <summary>
        /// Resets the map mode UI elements to their default state.
        /// </summary>
        public static void ResetMapUI()
        {
            MapModeText.text = "MAP MODE";
            MapModeTextGameObject.SetActive(false);

            MapSearchInputField.text = "";
            MapSearchGameObject.SetActive(false);
        }

        #endregion

        #region Star Map Visuals

        /// <summary>
        /// Sets the active state of various map elements.
        /// </summary>
        internal static void SetMapStuffActive(bool active)
        {
            var starmapBorder = SimGame.Starmap.Screen.transform.Find("RegionBorders").gameObject
                .GetComponent<StarmapBorders>();
            SimGame.Starmap.Screen.transform.Find("Background").gameObject.SetActive(active);

            // Hide the annoying yellow undertone
            if (active)
            {
                if (_oldTravelIntensity != null)
                    starmapBorder.travelIntensity = (float)_oldTravelIntensity;
            }
            else
            {
                _oldTravelIntensity ??= starmapBorder.travelIntensity;
                starmapBorder.travelIntensity = 0;

                SimGame.Starmap.Screen.ForceClickSystem((StarmapSystemRenderer)null);
                NavigationScreen.ResetSpecialIndicators();
            }

            SimGame.Starmap.Screen.RefreshBorders();
        }

        /// <summary>
        /// Dims the specified star system to the given dim level.
        /// </summary>
        internal static void DimSystem(string system, float dimLevel)
        {
            MPB.Clear();

            var systemRenderer = SimGame.Starmap.Screen.GetSystemRenderer(system);

            var starOuter = systemRenderer.starOuter;
            var starInner = systemRenderer.starInner;
            var starInnerUnvisited = systemRenderer.starInnerUnvisited;
            var newColor = systemRenderer.systemColor / dimLevel;

            // Setting outer star color
            MPB.SetColor("_Color", newColor);
            starOuter.SetPropertyBlock(MPB);

            // Setting inner star color
            MPB.SetColor("_Color", newColor * 2f);
            starInner.SetPropertyBlock(MPB);
            starInnerUnvisited.SetPropertyBlock(MPB);
        }

        /// <summary>
        /// Scales the specified star system to the given scale.
        /// </summary>
        internal static void ScaleSystem(string system, float scale)
        {
            var systemRenderer = SimGame.Starmap.Screen.GetSystemRenderer(system);

            //Main.Log.LogDebug($"Scaling {system} to {scale} -- old scale {systemRenderer.transform.localScale}");
            systemRenderer.transform.localScale = new Vector3(scale, scale, scale);
        }

        #endregion
    }
}