using BattleTech;
using RogueTechPerfFixes.Models;
using RogueTechPerfFixes.Utils;

namespace RogueTechPerfFixes.Patches
{
    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch(nameof(AbstractActor.IsFuryInspired))]
    [HarmonyPatch(MethodType.Getter)]
    public static class H_AbstractActor
    {
        public static bool Prepare()
        {
            return Mod.Settings.Patch.Vanilla;
        }

        public static bool Prefix(ref bool __result)
        {
            // If not in skirmish, return false immediately.
            if (UnityGameInstance.BattleTechGame.Simulation != null)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(AbstractActor), nameof(AbstractActor.HandleDeath))]
    public static class AbstractActor_HandleDeath
    {
        private static int counter = 0;

        public static bool GateActive = false;

        public static bool Prepare()
        {
            return Mod.Settings.Patch.LowVisibility;
        }

        [HarmonyPriority(900)]
        public static void Prefix(AbstractActor __instance)
        {
            VisibilityCacheGate.EnterGate();
            GateActive = true;
            counter = VisibilityCacheGate.GetCounter;
        }

        [HarmonyPriority(0)]
        public static void Postfix(AbstractActor __instance)
        {
            VisibilityCacheGate.ExitGate();
            GateActive = false;

            var exitCounter = VisibilityCacheGate.GetCounter;
            if (exitCounter < counter)
            {
                RTPFLogger.Debug?.Write($"Reset or unsymmetrical larger number of ExitGate() are call.");
            }
            else if (exitCounter > counter)
            {
                RTPFLogger.Error?.Write($"Fewer calls to ExitGate() than EnterGate().");
            }
        }
    }
}
