using System.Collections.Generic;
using BattleTech.Rendering;
using RogueTechPerfFixes.Utils;

namespace RogueTechPerfFixes.Patches;

/// <summary>
/// Allowed all newly added light sorted in a batch instead of sorting every time a new light is added.
/// </summary>
public static class H_BTLightController
{
    [HarmonyPatch(typeof(BTLightController), nameof(BTLightController.AddLight))]
    public static class H_AddLight
    {
        /// <summary>
        /// Check whether <see cref="BTLightController.InBatchProcess"/> is injected.
        /// </summary>
        /// <returns> Returns true, if the field is found in the class. </returns>
        public static bool Prepare()
        {
            return Mod.Settings.Patch.Vanilla;
        }

        public static bool Prefix(BTLight light, List<BTLight> ___lightList)
        {
            if (BTLightController.InBatchProcess)
            {
                BTLightController.LightAdded = true;
                if (!light.isInLightList)
                {
                    ___lightList.Add(light);
                    light.isInLightList = true;
                }

                return false;
            }

            return true;
        }
    }
}