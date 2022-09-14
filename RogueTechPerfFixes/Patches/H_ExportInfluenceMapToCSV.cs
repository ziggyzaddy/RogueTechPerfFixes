using BattleTech;
using Harmony;

namespace RogueTechPerfFixes.Patches
{
    [HarmonyPatch(typeof(InfluenceMapEvaluator), nameof(InfluenceMapEvaluator.ExportInfluenceMapToCSV))]
    public static class H_ExportInfluenceMapToCSV
    {
        public static bool Prepare()
        {
            return Mod.Settings.Patch.Vanilla;
        }

        /// <summary>
        /// Skip running an expansive logging method.
        /// </summary>
        /// <returns></returns>
        public static bool Prefix()
        {
            return false;
        }
    }
}
