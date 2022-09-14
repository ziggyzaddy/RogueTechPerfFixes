using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using RogueTechPerfFixes.Utils;

namespace RogueTechPerfFixes
{
    public static class Mod
    {
        private static Settings _settings;

        public static Settings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = JsonConvert.DeserializeObject<Settings>(GetSetting());

                return _settings;
            }

            set => _settings = value;
        }

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

        private static string GetSetting()
        {
            return File.ReadAllText(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    , "mod.json"));
        }
    }
}
