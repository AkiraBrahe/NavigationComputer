using HBS.Logging;
using NavigationComputer.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NavigationComputer
{
    public static class Main
    {
        internal static ILog Log { get; private set; }
        internal static ModSettings Settings { get; private set; }

        public static bool HasUnlockedFactionStores { get; private set; }
        public static bool HasTBD { get; private set; }

        public static void Init(string settings)
        {
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
                if (loadOrder.Contains("BTFactionStoreUnlock"))
                {
                    HasUnlockedFactionStores = true;
                }

                if (loadOrder.Contains("The_Big_Deal_Campaign_Add-on"))
                {
                    HasTBD = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }
    }
}