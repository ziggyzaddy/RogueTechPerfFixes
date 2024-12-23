using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using RogueTechPerfFixes.Utils;

namespace RogueTechPerfFixes
{
    public static class Mod
    {
        public static Settings Settings { get; private set; }

        public static void Init(string modDirectory, string settingsJSON)
        {
            try
            {
                RTPFLogger.InitCriticalLogger(modDirectory);
                Settings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
                HarmonyUtils.Harmony.PatchAll();
            }
            catch (Exception e)
            {
                RTPFLogger.LogCritical(e.ToString());
            }

        }
    }
}
