using System;
using BattleTech;
using Harmony;
using RogueTechPerfFixes.Models;

namespace RogueTechPerfFixes.HarmonyPatches
{
    public static class VisibilityCachePatches
    {
        public delegate AbstractActor OwningActor(VisibilityCache cache);

        public static OwningActor GetOwningActor =
            (OwningActor)Delegate.CreateDelegate(typeof(OwningActor), typeof(VisibilityCache).GetProperty("OwningActor", AccessTools.all).GetMethod);

        [HarmonyPatch(typeof(VisibilityCache), nameof(VisibilityCache.RebuildCache))]
        public static class VisibilityCache_RebuildCache
        {
            public static bool Prepare()
            {
                return Mod.Mod.Settings.Patch.LowVisibility;
            }

            // Lowest priority
            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(VisibilityCache __instance)
            {
                if (VisibilityCacheGate.Active)
                {
                    VisibilityCacheGate.AddActorToRefresh(GetOwningActor(__instance));
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(VisibilityCache), nameof(VisibilityCache.UpdateCacheReciprocal))]
        public static class VisibilityCache_UpdateCacheReciprocal
        {
            public static bool Prepare()
            {
                return Mod.Mod.Settings.Patch.LowVisibility;
            }

            // Lowest priority
            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(VisibilityCache __instance)
            {
                if (VisibilityCacheGate.Active)
                {
                    VisibilityCacheGate.AddActorToRefreshReciprocal(GetOwningActor(__instance));
                    return false;
                }

                return true;
            }
        }
    }
}
