using BattleTech.Rendering;

namespace RogueTechPerfFixes.Patches
{
    [HarmonyPatch(typeof(MissileLauncherEffect), "Update")]
    public static class H_MissileLauncherEffect__Update
    {
        public static bool Prepare()
        {
            return Mod.Settings.Patch.Vanilla;
        }

        [HarmonyPriority(Priority.First)]
        public static void Prefix()
        {
            BTLightController.InBatchProcess = true;
        }

        public static void Postfix()
        {
            BTLightController.InBatchProcess = false;
            if (BTLightController.LightAdded)
            {
                BTLightController.lightList.Sort();
                BTLightController.LightAdded = false;
            }
        }
    }
}
