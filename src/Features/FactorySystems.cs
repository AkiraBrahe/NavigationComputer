using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BEXTimeline;
using NavigationComputer.Utils;
using NavigationComputer.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable IDE0028
namespace NavigationComputer.Features
{
    /// <summary>
    /// Manages the tooltips and styles of factory systems on the star map.
    /// </summary>
    public static class FactorySystems
    {
        #region Factory Production Cache

        private const string Separator = "\n<size=80%>---</size>";
        private const string FactoryProductionMarker = "<b>FACTORY PRODUCTION:</b>";
        private static readonly Dictionary<string, FactoryProductionCacheEntry> FactoryProductionCache = new(StringComparer.Ordinal);
        private static int _lastYearUpdated = 1999;

        private sealed class FactoryProductionCacheEntry
        {
            public int Year;
            public bool IsReady;
            public List<MechDef> MechDefs = [];
            public List<VehicleDef> VehicleDefs = [];
            public readonly List<Action<List<MechDef>, List<VehicleDef>>> ReadyCallbacks = [];
        }

        #endregion

        #region Factory Production Data

        /// <summary>
        /// Refreshes the factory production cache when the map is updated.
        /// </summary>
        [HarmonyPatch(typeof(UpdateOwnership), "UpdateTheMap")]
        public static class UpdateOwnership_UpdateTheMap
        {
            [HarmonyPostfix]
            public static void Postfix(SimGameState simGame) => RefreshFactoryProductionCache(simGame);

            private static void RefreshFactoryProductionCache(SimGameState simGame)
            {
                int currentYear = simGame.CurrentDate.Year;
                if (currentYear == _lastYearUpdated) return;

                _lastYearUpdated = currentYear;

                foreach (var systemList in simGame.FactionStoreStarSystemsDictionary.Values)
                {
                    foreach (var system in systemList)
                    {
                        FactoryProductionCache.TryGetValue(system.ID, out var cachedEntry);
                        if (cachedEntry != null && cachedEntry.Year == currentYear) continue;

                        var entry = new FactoryProductionCacheEntry { Year = currentYear };
                        if (cachedEntry != null)
                        {
                            entry.ReadyCallbacks.AddRange(cachedEntry.ReadyCallbacks);
                        }

                        FactoryProductionCache[system.ID] = entry;
                        LoadFactoryDefinitions(simGame, system, (mechDefs, vehicleDefs) =>
                            CompleteFactoryProductionLoad(system, entry, mechDefs, vehicleDefs));
                    }
                }
            }
        }

        /// <summary>
        /// Loads the mechs and vehicles produced by the factory system.
        /// </summary>
        /// <remarks>
        /// The logic uses a callback-based approach to handle the asynchronous nature of item collection generation.
        /// </remarks>
        private static void LoadFactoryDefinitions(SimGameState simGame, StarSystem system, Action<List<MechDef>, List<VehicleDef>> callback)
        {
            var factoryItemCollections = GetFactoryItemCollections(simGame, system);
            if (factoryItemCollections.Count == 0)
            {
                callback([], []);
                return;
            }

            var aggregateResultItems = new List<ShopDefItem>();
            int collectionsPending = factoryItemCollections.Count;

            void SingleCollectionCallback(ItemCollectionResult result)
            {
                if (result?.items != null)
                {
                    aggregateResultItems.AddRange(result.items);
                }

                collectionsPending--;
                if (collectionsPending > 0) return;

                var mechDefs = new List<MechDef>();
                var vehicleDefs = new List<VehicleDef>();
                foreach (var item in aggregateResultItems.Where(item => item.Type is ShopItemType.Mech))
                {
                    if (string.IsNullOrEmpty(item.ID)) continue;

                    if (item.ID.StartsWith("mechdef_")
                        && simGame.DataManager.MechDefs.TryGet(item.ID, out var mechDef))
                    {
                        mechDefs.Add(mechDef);
                    }
                    else if (item.ID.StartsWith("vehicledef_")
                             && simGame.DataManager.VehicleDefs.TryGet(item.ID, out var vehicleDef))
                    {
                        vehicleDefs.Add(vehicleDef);
                    }
                }

                callback(mechDefs, vehicleDefs);
            }

            foreach (var collectionDef in factoryItemCollections)
            {
                simGame.ItemCollectionResultGen.GenerateItemCollection(collectionDef, -1, SingleCollectionCallback);
            }
        }

        /// <summary>
        /// Retrieves the item collection definitions from a factory system.
        /// </summary>
        /// <remarks>
        /// To optimize the callback, we only process item collections that contain unit definitions since GenerateItemCollection() recursively finds all nested item collections.
        /// </remarks>
        private static List<ItemCollectionDef> GetFactoryItemCollections(SimGameState simGame, StarSystem system)
        {
            var factoryItemCollections = new HashSet<string>(
                system.Def.FactionShopItems?
                    .Where(id => !string.IsNullOrEmpty(id) && id.StartsWith("itemCollection_factory"))
                    .Select(id => id.Replace("factoryHolder", "factory")) ?? []
            );

            if (system.Name == "Outreach")
            {
                factoryItemCollections.Remove("itemCollection_factory_Outreach");
                factoryItemCollections.Add("itemCollection_systemStores_Mechs_Dragoons");
                factoryItemCollections.Add("itemCollection_systemStores_Vehicles_Dragoons");

                if (simGame.CurrentDate.Year >= 3055
                    && system.Def.SystemShopItems != null
                    && system.Def.SystemShopItems.Contains("itemCollection_special_GAL"))
                {
                    factoryItemCollections.Add("itemCollection_special_GAL");
                }
            }
            else if (system.Name == "New Valencia"
                     && simGame.CurrentDate.Year >= 3030
                     && system.Def.SystemShopItems != null
                     && system.Def.SystemShopItems.Contains("itemCollection_factory_MarauderII"))
            {
                factoryItemCollections.Add("itemCollection_factory_MarauderII");
            }

            return [.. factoryItemCollections
                .Where(simGame.DataManager.ItemCollectionDefs.Exists)
                .Select(simGame.DataManager.ItemCollectionDefs.Get)];
        }

        /// <summary>
        /// Completes the factory production load by populating the cache entry with the loaded definitions.
        /// </summary>
        private static void CompleteFactoryProductionLoad(StarSystem system, FactoryProductionCacheEntry entry,
            List<MechDef> mechDefs, List<VehicleDef> vehicleDefs)
        {
            if (!FactoryProductionCache.TryGetValue(system.ID, out var currentEntry)
                || !ReferenceEquals(currentEntry, entry))
                return;

            entry.MechDefs = mechDefs;
            entry.VehicleDefs = vehicleDefs;
            entry.IsReady = true;

            string details = system.Def.Description.Details;
            if ((mechDefs.Count > 0 || vehicleDefs.Count > 0)
                && (details == null || !details.Contains(FactoryProductionMarker)))
            {
                ProcessFactorySystem(system, mechDefs, vehicleDefs);
            }

            var callbacks = entry.ReadyCallbacks.ToList();
            entry.ReadyCallbacks.Clear();
            foreach (var callback in callbacks)
            {
                callback(entry.MechDefs, entry.VehicleDefs);
            }
        }

        /// <summary>
        /// Requests cached factory definitions for a system, executing a callback when they become available.
        /// </summary>
        private static bool RequestCachedFactoryDefinitions(string systemId, Action<List<MechDef>, List<VehicleDef>> callback)
        {
            if (!FactoryProductionCache.TryGetValue(systemId, out var entry))
                return false;

            if (entry.IsReady)
            {
                callback(entry.MechDefs, entry.VehicleDefs);
            }
            else
            {
                entry.ReadyCallbacks.Add(callback);
            }

            return true;
        }

        #endregion

        #region Factory System Tooltips

        /// <summary>
        /// Processes a factory system by building and appending production text for mechs and vehicles.
        /// </summary>
        private static void ProcessFactorySystem(StarSystem system, List<MechDef> mechDefs, List<VehicleDef> vehicleDefs)
        {
            var mechText = BuildProductionText(mechDefs, def => def.Chassis.Description.Name, def => def.Chassis.Tonnage);
            var vehicleText = BuildProductionText(vehicleDefs, def => def.GetBaseChassis(), def => def.Chassis.Tonnage);

            if (mechText.Any() || vehicleText.Any())
            {
                AppendProductionText(system, mechText, vehicleText);
            }
        }

        /// <summary>
        /// Builds production text for mechs and vehicles, sorted by tonnage and name.
        /// </summary>
        private static List<string> BuildProductionText<T>(IEnumerable<T> items, Func<T, string> name, Func<T, float> tonnage)
        {
            var unitNames = items
                .OrderByDescending(tonnage)
                .ThenBy(name)
                .Select(def => CreateUnbreakableUnitString(name(def), tonnage(def)))
                .Distinct()
                .ToList();

            if (!unitNames.Any()) return [];

            unitNames[0] = $"\u00A0{unitNames[0]}";
            return [string.Join(", ", unitNames)];
        }

        /// <summary>
        /// Appends production text to a system's description, handling both mechs and vehicles.
        /// </summary>
        private static void AppendProductionText(StarSystem system, List<string> mechText, List<string> vehicleText)
        {
            bool hasMechs = mechText.Any();
            bool hasVehicles = vehicleText.Any();

            var finalTextBuilder = new StringBuilder();
            finalTextBuilder.AppendLine(Separator);
            finalTextBuilder.AppendLine(FactoryProductionMarker);

            if (hasMechs)
            {
                foreach (string line in mechText)
                {
                    if (line.StartsWith("\u00A0"))
                    {
                        string wrappedLine = RichTextWrapper.WrapLine($"\u00A0<b>Mechs:</b>{line}", 52);
                        finalTextBuilder.AppendLine(wrappedLine);
                    }
                    else
                    {
                        string wrappedLine = RichTextWrapper.WrapLine(line, 52);
                        finalTextBuilder.AppendLine(wrappedLine);
                    }
                }
            }

            if (hasVehicles)
            {
                foreach (string line in vehicleText)
                {
                    if (line.StartsWith("\u00A0"))
                    {
                        string wrappedLine = RichTextWrapper.WrapLine($"\u00A0<b>Vehicles:</b>{line}", 52);
                        finalTextBuilder.AppendLine(wrappedLine);
                    }
                    else
                    {
                        string wrappedLine = RichTextWrapper.WrapLine(line, 52);
                        finalTextBuilder.AppendLine(wrappedLine);
                    }
                }
            }

            system.Def.Description.Details += finalTextBuilder.ToString();
        }

        private static string CreateUnbreakableUnitString(string name, float tonnage) => $"{name} ({tonnage}t)".Replace(' ', '\u00A0');

        /// <summary>
        /// Removes factory production text from the system summary.
        /// </summary>
        [HarmonyPatch(typeof(SGSystemViewPopulator), "UpdateRoutedSystem")]
        public static class SGSystemViewPopulator_UpdateRoutedSystem_Cleanup
        {
            [HarmonyPostfix]
            public static void Postfix(SGSystemViewPopulator __instance)
            {
                if (__instance.SystemDescriptionFields == null) return;

                foreach (var descriptionField in __instance.SystemDescriptionFields)
                {
                    if (descriptionField == null) continue;

                    string summary = descriptionField.text;
                    if (string.IsNullOrEmpty(summary)) continue;

                    int sepIndex = summary.IndexOf(Separator, StringComparison.Ordinal);
                    if (sepIndex <= 0) continue;

                    __instance.SetField(descriptionField, summary.Substring(0, sepIndex).TrimEnd());
                }
            }
        }

        #endregion

        #region Factory Production Panel

        private static GameObject _tagPanel;
        private static LocalizableText _tagTabText;
        private static Button _tagTabButton;
        private static GameObject _factoryPanel;
        private static LocalizableText _factoryTabText;
        private static Button _factoryTabButton;

        private static bool ShouldHideProduction => Main.Settings.MapModes.HideNonAlliedProduction && !Main.HasUnlockedFactionStores;

        /// <summary>
        /// Adds a factory panel to the system-view UI and loads the production definitions into the panel.
        /// </summary>
        [HarmonyPatch(typeof(SGSystemViewPopulator), "UpdateRoutedSystem")]
        public static class SGSystemViewPopulator_UpdateRoutedSystem
        {
            [HarmonyPrepare]
            public static bool Prepare() => Main.Settings.MapModes.ShowProductionTab;

            [HarmonyPostfix]
            public static void Postfix(SGSystemViewPopulator __instance)
            {
                if (_tagPanel == null)
                {
                    _tagPanel = __instance.gameObject.FindInactive("uixPrfPanl_NAV_tagsList-Element-MANAGED");
                }

                if (_tagPanel == null)
                {
                    Main.Log.LogError("Could not find the system-view Tags panel.");
                    return;
                }

                if (_factoryPanel == null)
                {
                    var tabParent = _tagPanel.transform.parent;
                    _factoryPanel = UnityEngine.Object.Instantiate(_tagPanel.gameObject, tabParent);
                    _factoryPanel.name = "uixPrfPanl_NAV_factoryProduction-Element-MANAGED";
                    _factoryPanel.SetActive(false);
                }

                if (_tagTabButton == null || _factoryTabButton == null)
                {
                    if (!SetupTabs(_tagPanel, _factoryPanel)) return;
                }

                var productionWidget = _factoryPanel.GetComponent<FactoryProductionPanelWidget>()
                    ?? _factoryPanel.AddComponent<FactoryProductionPanelWidget>();
                if (!productionWidget.Initialize()) return;

                var system = __instance.starSystem;
                var simGame = __instance.simState;
                if (system == null || simGame == null)
                {
                    Main.Log.LogError("System-view factory production could not find the routed system or SimGameState.");
                    return;
                }

                var factoryOwner = system.Def.FactionShopOwnerValue.IsInvalidUnset
                    ? system.Def.OwnerValue : system.Def.FactionShopOwnerValue;
                bool isAlliedSystem = simGame.IsFactionAlly(factoryOwner, null);
                if (!isAlliedSystem && ShouldHideProduction)
                {
                    ShowTagTab();
                    SetProductionTabAvailable(false);
                    productionWidget.HideProduction();
                    return;
                }

                bool hasFactoryProduction = FactoryProductionCache.TryGetValue(system.ID, out var cacheEntry)
                    && cacheEntry.Year == simGame.CurrentDate.Year;
                SetProductionTabAvailable(hasFactoryProduction);
                productionWidget.SetSystem(system.ID);

                if (!hasFactoryProduction)
                {
                    ShowTagTab();
                    return;
                }

                RequestCachedFactoryDefinitions(system.ID, (mechDefs, vehicleDefs) =>
                    productionWidget?.ShowProduction(system.ID, mechDefs, vehicleDefs));
                ShowFactoryTab();
            }
        }

        /// <summary>
        /// Sets up the tab buttons for the system-view UI.
        /// </summary>
        private static bool SetupTabs(GameObject tagPanel, GameObject factoryPanel)
        {
            var tagText = tagPanel.FindInactive("summaryTitle_text")?.GetComponent<LocalizableText>();
            var factoryText = factoryPanel.FindInactive("summaryTitle_text")?.GetComponent<LocalizableText>();

            if (tagText == null || factoryText == null)
            {
                Main.Log.LogError("Could not find the system-view tab title text.");
                return false;
            }

            _tagTabText = tagText;
            _tagTabButton = tagText.GetComponent<Button>() ?? tagText.gameObject.AddComponent<Button>();
            _tagTabButton.onClick.RemoveAllListeners();
            _tagTabButton.onClick.AddListener(ShowFactoryTab);

            _factoryTabText = factoryText;
            _factoryTabText.text = "Production";
            _factoryTabButton = factoryText.GetComponent<Button>() ?? factoryText.gameObject.AddComponent<Button>();
            _factoryTabButton.onClick.RemoveAllListeners();
            _factoryTabButton.onClick.AddListener(ShowTagTab);
            return true;
        }

        private static void SetProductionTabAvailable(bool isAvailable) => _tagTabButton.interactable = isAvailable;

        private static void ShowTagTab()
        {
            _tagPanel.SetActive(true);
            _factoryPanel.SetActive(false);
        }

        private static void ShowFactoryTab()
        {
            _tagPanel.SetActive(false);
            _factoryPanel.SetActive(true);
        }

        #endregion

        #region Factory System Styles

        public struct StoreStyleConfig
        {
            public string LogoId;
            public string HexColor;
        }

        public static readonly Dictionary<string, StoreStyleConfig> CustomStoreStyles = new()
        {
            ["WolfsDragoons"] = new StoreStyleConfig { LogoId = "uixTxrLogo_WDragoons", HexColor = "#C00008" },
            ["Outworld"] = new StoreStyleConfig { LogoId = "uixTxrLogo_Outworld", HexColor = "#9F522D" },
            ["Marian"] = new StoreStyleConfig { LogoId = "uixTxrLogo_Marian", HexColor = "#F78549" }
        };

        /// <summary>
        /// Applies custom faction styles to factory system indicators on the star map.
        /// </summary>
        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetFactionCapital")]
        public static class StarmapSystemRenderer_SetFactionCapital
        {
            [HarmonyPostfix]
            public static void Postfix(StarmapSystemRenderer __instance, FactionValue faction)
            {
                if (MapModesUI.SimGame != null && __instance.currentFactionObj != null && CustomStoreStyles.TryGetValue(faction.Name, out var config))
                {
                    ApplyStyle(__instance.currentFactionObj, config, MapModesUI.SimGame);
                }
            }

            private static void ApplyStyle(GameObject indicatorObject, StoreStyleConfig config, SimGameState simState)
            {
                var factionLogo = indicatorObject.transform.Find("factionLogo")?.gameObject;
                if (factionLogo != null)
                {
                    simState.RequestItem<Texture2D>(
                        config.LogoId,
                        (texture2D) =>
                        {
                            if (texture2D != null && factionLogo != null)
                            {
                                factionLogo.GetComponent<ParticleSystemRenderer>().material.mainTexture = texture2D;
                            }
                        },
                        BattleTechResourceType.Texture2D
                    );
                }

                var techCircle = indicatorObject.transform.Find("techCircle")?.gameObject;
                if (techCircle != null && ColorUtility.TryParseHtmlString(config.HexColor, out var customColor))
                {
                    var mainModule = techCircle.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = new ParticleSystem.MinMaxGradient(customColor);
                }
            }
        }

        #endregion
    }
}