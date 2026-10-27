using HBS.Logging;
using NavigationComputer.Features;
using NavigationComputer.Features.MapModes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NavigationComputer
{
    public static class Main
    {
        internal static string ModDir;
        internal static ILog Log { get; private set; }
        internal static ModSettings Settings { get; private set; }

        public static bool HasUnlockedFactionStores { get; private set; }
        public static bool HasTBD { get; private set; }
        public static bool HasLorePacks { get; private set; }

        public static void Init(string directory, string settings)
        {
            ModDir = directory;
            Log = Logger.GetLogger("NavigationComputer", LogLevel.Debug);

            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(settings);
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "io.github.mpstark.NavigationComputer");
                MapModesUI.Setup();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        public static void FinishedLoading(List<string> loadOrder)
        {
            try
            {
                HasUnlockedFactionStores = loadOrder.Contains("BTFactionStoreUnlock");
                HasTBD = loadOrder.Contains("The_Big_Deal_Campaign_Add-on");
                HasLorePacks = loadOrder.Contains("Lore_Pack_4SW");

                MergeLoreCampaigns(Settings);
                MergeSearchableTags(Settings);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        private static void MergeLoreCampaigns(ModSettings settings)
        {
            if (!HasLorePacks || settings.LoreCampaigns == null)
                return;

            foreach (var kvp in settings.LoreCampaigns)
            {
                var campaignSettings = kvp.Value;
                if (campaignSettings?.BranchIds == null
                    || campaignSettings.BranchIds.Length == 0
                    || campaignSettings.StartDate == default
                    || campaignSettings.EndDate == default
                    || campaignSettings.EndDate < campaignSettings.StartDate)
                {
                    Log.LogError($"Invalid lore campaign settings for '{kvp.Key}'. "
                        + "Configure BranchIds and valid StartDate/EndDate values.");
                    continue;
                }

                Flashpoint.CampaignTrackerDefinitions[kvp.Key] = new Flashpoint.CampaignData
                {
                    BranchIds = campaignSettings.BranchIds,
                    StartDate = campaignSettings.StartDate,
                    EndDate = campaignSettings.EndDate
                };
            }
        }

        private static void MergeSearchableTags(ModSettings settings)
        {
            if (settings.SearchableTags == null)
                return;

            foreach (var kvp in settings.SearchableTags)
            {
                Search.TagIdToFriendlyName[kvp.Key] = kvp.Value;
            }
        }
    }
}