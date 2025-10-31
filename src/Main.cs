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

        public class ModSettings
        {
            public Dictionary<string, string> SearchableTags = [];
        }

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
    }
}
