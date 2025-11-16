using BattleTech;
using BEXTimeline;
using NavigationComputer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NavigationComputer.Features
{
    public static class FactionStores
    {
        #region Faction Store Descriptions

        public static DateTime LastYearUpdated = new(1999, 1, 1);

        /// <summary>
        ///  Updates factory system descriptions to include all the mechs and vehicles produced at this system.
        /// </summary>
        [HarmonyPatch(typeof(UpdateOwnership), "UpdateTheMap")]
        public static class UpdateOwnership_UpdateTheMap
        {
            [HarmonyPostfix]
            public static void Postfix(SimGameState simGame)
            {
                if (simGame.CurrentDate.Year <= LastYearUpdated.Year)
                    return;

                LastYearUpdated = new DateTime(simGame.CurrentDate.Year, 1, 1);

                foreach (var systemList in simGame.FactionStoreStarSystemsDictionary.Values)
                {
                    foreach (var system in systemList)
                    {
                        if (system.Def.Description.Details.Contains("<b>LOCAL PRODUCTION:</b>"))
                            continue;

                        var factoryItemCollections = GetFactoryItemCollections(simGame, system);
                        if (factoryItemCollections.Count == 0)
                            continue;

                        var aggregateResultItems = new List<ShopDefItem>();
                        int collectionsPending = factoryItemCollections.Count;

                        void singleCollectionCallback(ItemCollectionResult result)
                        {
                            if (result?.items != null)
                            {
                                aggregateResultItems.AddRange(result.items);
                            }

                            collectionsPending--;

                            if (collectionsPending <= 0)
                            {
                                ProcessFactorySystem(system, aggregateResultItems, simGame);
                            }
                        }

                        foreach (var collectionDef in factoryItemCollections)
                        {
                            //Main.Log.LogDebug($"Checking {collectionDef.ID} for {system.Name}.");
                            simGame.ItemCollectionResultGen.GenerateItemCollection(collectionDef, -1, singleCollectionCallback);
                        }
                    }
                }
            }

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

                    if (simGame.CurrentDate >= new DateTime(3055, 1, 1) &&
                        system.Def.SystemShopItems != null && system.Def.SystemShopItems.Contains("itemCollection_special_GAL"))
                    {
                        factoryItemCollections.Add("itemCollection_special_GAL");
                    }
                }
                else if (system.Name == "New Valencia" && simGame.CurrentDate >= new DateTime(3030, 1, 1) &&
                         system.Def.SystemShopItems != null && system.Def.SystemShopItems.Contains("itemCollection_factory_MarauderII"))
                {
                    factoryItemCollections.Add("itemCollection_factory_MarauderII");
                }

                var validCollections = factoryItemCollections
                    .Where(simGame.DataManager.ItemCollectionDefs.Exists)
                    .Select(simGame.DataManager.ItemCollectionDefs.Get)
                    .ToList();

                return validCollections;
            }

            private static void ProcessFactorySystem(StarSystem system, List<ShopDefItem> items, SimGameState simGame)
            {
                List<MechDef> mechDefs = [];
                List<VehicleDef> vehicleDefs = [];

                foreach (var item in items.Where(i => i.Type is ShopItemType.Mech))
                {
                    if (item.ID.StartsWith("mechdef_"))
                    {
                        if (!simGame.DataManager.MechDefs.TryGet(item.ID, out var mechDef)) continue;
                        mechDefs.Add(mechDef);
                    }
                    else if (item.ID.StartsWith("vehicledef_"))
                    {
                        if (!simGame.DataManager.VehicleDefs.TryGet(item.ID, out var vehicleDef)) continue;
                        vehicleDefs.Add(vehicleDef);
                    }
                }

                var mechText = BuildProductionText(mechDefs, def => def.Chassis.weightClass, def => def.Name, def => def.Chassis.Tonnage);
                var vehicleText = BuildProductionText(vehicleDefs, def => def.Chassis.weightClass, def => def.Description.Name, def => def.Chassis.Tonnage);

                if (mechText.Any() || vehicleText.Any())
                {
                    AppendProductionText(system, mechText, vehicleText);
                }
            }

            private static List<string> BuildProductionText<T>(IEnumerable<T> items, Func<T, WeightClass> weightClass, Func<T, string> name, Func<T, float> tonnage)
            {
                return [.. items.GroupBy(weightClass)
                    .OrderByDescending(group => group.Key)
                    .Select(group =>
                    {
                        var unitNames = group
                            .OrderBy(name)
                            .Select(def => CreateUnbreakableUnitString(name(def), tonnage(def)))
                            .Distinct()
                            .ToList();

                        if (!unitNames.Any()) return string.Empty;

                        string header = $"{ToProperCase(group.Key.ToString())}:";
                        unitNames[0] = $"\u00A0\u00A0<b>{header}</b>\u00A0{unitNames[0]}";
                        return string.Join(", ", unitNames);
                    }).Where(s => !string.IsNullOrEmpty(s))];
            }

            private static void AppendProductionText(StarSystem system, List<string> mechText, List<string> vehicleText)
            {
                bool hasMechs = mechText.Any();
                bool hasVehicles = vehicleText.Any();

                var finalTextBuilder = new StringBuilder();
                finalTextBuilder.AppendLine("\n<size=80%>---</size>");
                finalTextBuilder.AppendLine("<b>LOCAL PRODUCTION:</b>");

                if (hasMechs)
                {
                    if (hasVehicles) finalTextBuilder.AppendLine("\u00A0<b>Mechs:</b>");
                    foreach (string line in mechText)
                    {
                        string wrappedLine = RichTextWrapper.WrapLine(line, 54);
                        finalTextBuilder.AppendLine(wrappedLine);
                    }
                }

                if (hasVehicles)
                {
                    if (hasMechs) finalTextBuilder.AppendLine("\n\u00A0<b>Vehicles:</b>");
                    foreach (string line in vehicleText)
                    {
                        string wrappedLine = RichTextWrapper.WrapLine(line, 54);
                        finalTextBuilder.AppendLine(wrappedLine);
                    }
                }

                system.Def.Description.Details += finalTextBuilder.ToString();
            }

            private static string CreateUnbreakableUnitString(string name, float tonnage) => $"{name} ({tonnage}t)".Replace(' ', '\u00A0');

            private static string ToProperCase(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1).ToLower();
        }

        #endregion

        #region Faction Store Styles

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
        /// Applies custom faction styles to faction store indicators on the star map.
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