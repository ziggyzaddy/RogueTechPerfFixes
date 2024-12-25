using System.Reflection;
using Newtonsoft.Json;

namespace RogueTechPerfFixes;

public static class Mod
{
    public static Settings Settings { get; private set; }

    public static void Init(string settingsJSON)
    {
        Settings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), nameof(RogueTechPerfFixes));
    }
}