using HBS.Logging;
using NavigationComputer.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace NavigationComputer
{
    public static class Main
    {
        internal static ILog Log { get; private set; }
        internal static ModSettings Settings { get; private set; }

        public static bool BTFactionStoreUnlockDetected =>
            AppDomain.CurrentDomain.GetAssemblies().Any(asm => asm.GetName().Name.Equals("BTFactionStoreUnlock"));

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
