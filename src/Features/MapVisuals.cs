using BattleTech;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NavigationComputer.Features
{
    /// <summary>
    /// Manages visual enhancements to the star map, such as highlighting inhabited systems and resizing star clusters.
    /// </summary>
    public static class MapVisuals
    {
        public class ClusterData
        {
            public string Name { get; set; }
            public float? Width { get; set; }
            public float? Height { get; set; }
        }

        public static readonly Dictionary<string, ClusterData> StarClusters = new()
        {
            { "starsystemdef_BrocchisCluster", new ClusterData { Name = "Brocchi's Cluster", Width = 6.884f, Height = 7.451f } },
            { "starsystemdef_ChaineCluster", new ClusterData { Name = "Chaine Cluster", Width = 5.634f, Height = 5.637f } },
            { "starsystemdef_EndersCluster", new ClusterData { Name = "Enders Cluster", Width = 4.365f, Height = 4.365f } },
            { "starsystemdef_EritCluster", new ClusterData { Name = "Erit Cluster", Width = null, Height = null } },
            { "starsystemdef_HyadesCluster", new ClusterData { Name = "Hyades Cluster", Width = 13.819f, Height = 14.107f } },
            { "starsystemdef_PiratesHavenCluster", new ClusterData { Name = "Pirates Haven Cluster", Width = 27.424f, Height = 27.422f } },
            { "starsystemdef_PleiadesCluster", new ClusterData { Name = "Pleiades Cluster", Width = 14.153f, Height = 14.218f } },
            { "starsystemdef_ThetaCarinaeCluster", new ClusterData { Name = "Theta Carinae Cluster", Width = null, Height = null } },
            { "starsystemdef_TrznadelCluster", new ClusterData { Name = "Trznadel Cluster", Width = null, Height = null } },
        };

        /// <summary>
        /// Highlights inhabited systems and make high-population systems brighter.
        /// Also makes star clusters larger and dimmer so they encompass all their stars.
        /// </summary>
        [HarmonyPatch(typeof(StarmapSystemRenderer), "Init")]
        public static class StarmapSystemRenderer_Init
        {
            [HarmonyPrepare]
            public static bool Prepare() => Main.Settings.MapVisuals.HighlightInhabitedSystems || Main.Settings.MapVisuals.HighlightStarClusters;

            [HarmonyPostfix]
            public static void Postfix(StarmapSystemRenderer __instance)
            {
                // Handle star clusters first
                if (StarClusters.ContainsKey(__instance.system.System.ID))
                {
                    if (Main.Settings.MapVisuals.HighlightStarClusters)
                        ResizeAndDimStarCluster(__instance);
                    return;
                }

                // Handle inhabited systems
                if (Main.Settings.MapVisuals.HighlightInhabitedSystems)
                {
                    HighlightSystemByPopulation(__instance);
                }
            }

            public static void HighlightSystemByPopulation(StarmapSystemRenderer systemRenderer)
            {
                bool isAbandoned = systemRenderer.system.System.Def.Tags.Contains("planet_other_empty");
                float starBrightness = isAbandoned ? 1f : 1.5f;

                if (Main.Settings.MapVisuals.ShowPopulationLevels)
                {
                    starBrightness = systemRenderer.system.System.Def.Tags.items.FirstOrDefault(tag => tag.StartsWith("planet_pop_")) switch
                    {
                        "planet_pop_large" => 2.00f,
                        "planet_pop_medium" => 1.75f,
                        "planet_pop_small" => 1.50f,
                        "planet_pop_none" => 1.25f,
                        _ => 1.00f,
                    };
                }

                SetStarBrightness(systemRenderer.starInner, systemRenderer.systemColor, starBrightness);
                SetStarBrightness(systemRenderer.starInnerUnvisited, systemRenderer.systemColor, starBrightness);
            }

            public static void ResizeAndDimStarCluster(StarmapSystemRenderer systemRenderer)
            {
                string systemID = systemRenderer.system.System.ID;
                float targetScale = 2f; // default value

                if (StarClusters.TryGetValue(systemID, out var clusterData)
                    && clusterData.Width.HasValue
                    && clusterData.Height.HasValue)
                {
                    targetScale = Mathf.Max(clusterData.Width.Value, clusterData.Height.Value) * 0.7f;
                }

                systemRenderer.starInnerUnvisited.gameObject.transform.localScale = Vector3.one * targetScale;
                systemRenderer.starInner.gameObject.SetActive(false);
                systemRenderer.starInnerUnvisited.gameObject.SetActive(true);
                systemRenderer.transform.SetAsLastSibling(); // move to background

                float starBrightness = 0.25f;
                SetStarBrightness(systemRenderer.starInnerUnvisited, systemRenderer.systemColor, starBrightness);
            }

            private static void SetStarBrightness(Renderer renderer, Color baseColor, float brightness)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", baseColor * brightness);
                renderer.SetPropertyBlock(mpb);
            }
        }

        /// <summary>
        /// Modifies star visibility to differentiate inhabited and abandoned systems instead of visited and unvisited.
        /// </summary>
        [HarmonyPatch(typeof(StarmapSystemRenderer), "SetStarVisibility")]
        public static class StarmapSystemRenderer_SetStarVisibility
        {
            [HarmonyPrepare]
            public static bool Prepare() => Main.Settings.MapVisuals.HighlightInhabitedSystems;

            [HarmonyPrefix]
            public static bool Prefix(ref bool __runOriginal, StarmapSystemRenderer __instance)
            {
                bool isAbandoned = __instance.system.System.Def.Tags.Contains("planet_other_empty");
                __instance.starInner.gameObject.SetActive(!isAbandoned);
                __instance.starInnerUnvisited.gameObject.SetActive(isAbandoned);

                __runOriginal = false;
                return false;
            }
        }
    }
}