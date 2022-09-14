using BattleTech;
using Harmony;
using RogueTechPerfFixes.Models;
using RogueTechPerfFixes.Utils;

namespace RogueTechPerfFixes.HarmonyPatches
{
    public static class H_CombatGameState
    {
        private const string ACTIVE_GATE = "{0} has active visibility cache gate.\n";

        [HarmonyPatch(typeof(CombatGameState), nameof(CombatGameState.Update))]
        public static class H_Update
        {
            public static bool Prepare()
            {
                return Mod.Mod.Settings.Patch.LowVisibility;
            }

            public static void Postfix()
            {
                bool error = false;

                if (AbstractActor_HandleDeath.GateActive)
                {
                    error = true;
                    RTPFLogger.Error?.Write($"Something has gone wrong in handling actor death, resetting VisibilityCacheGate.");
                }

                if (H_AuraActorBody.GateActive)
                {
                    error = true;
                    RTPFLogger.Error?.Write(string.Format(ACTIVE_GATE, nameof(H_AuraActorBody)));
                }

                if (H_EffectManager.H_OnRoundEnd.GateActive)
                {
                    error = true;
                    RTPFLogger.Error?.Write(string.Format(ACTIVE_GATE, nameof(H_EffectManager.H_OnRoundEnd)));
                }

                if (error)
                    VisibilityCacheGate.ExitAll();
            }
        }

        [HarmonyPatch(typeof(CombatGameState), nameof(CombatGameState.OnCombatGameDestroyed))]
        public static class H_OnCombatGameDestroyed
        {
            public static bool Prepare()
            {
                return Mod.Mod.Settings.Patch.LowVisibility;
            }

            public static void Postfix()
            {
                VisibilityCacheGate.Reset();
            }
        }
    }
}
