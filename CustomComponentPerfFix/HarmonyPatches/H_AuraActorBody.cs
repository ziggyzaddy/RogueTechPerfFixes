using CustomActivatableEquipment;
using Harmony;
using RogueTechPerfFixes.Models;
using RogueTechPerfFixes.Utils;

namespace RogueTechPerfFixes.HarmonyPatches
{
    public static class H_AuraActorBody
    {
        private static int _counter = 0;

        public static bool GateActive { get; private set; }

        /// <summary>
        /// When reapplying all effects, existing effects are removed one by one, which in return triggers
        /// a refreshing of the visibility cache for every removed effect. This patch is to pool all the refreshing
        /// work into one place.
        /// </summary>
        [HarmonyPatch(typeof(AuraActorBody), nameof(AuraActorBody.ReapplyAllEffects))]
        public static class H_ReapplyAllEffects
        {
            public static bool Prepare()
            {
                return Mod.Mod.Settings.Patch.LowVisibility && Mod.Mod.Settings.Patch.CustomActivatableEquipment;
            }

            public static void Prefix()
            {
                VisibilityCacheGate.EnterGate();
                GateActive = true;

                _counter = VisibilityCacheGate.GetCounter;
                RTPFLogger.Debug?.Write($"Enter visibility cache gate in {typeof(H_ReapplyAllEffects).FullName}:{nameof(Prefix)}\n");
            }

            public static void Postfix()
            {
                VisibilityCacheGate.ExitGate();
                GateActive = false;

                Utils.Utils.CheckExitCounter($"Fewer calls made to ExitGate() when reaches {typeof(H_ReapplyAllEffects).FullName}:{nameof(Postfix)}.\n", _counter);
                RTPFLogger.Debug?.Write($"Exit visibility cache gate in {typeof(H_ReapplyAllEffects).FullName}:{nameof(Postfix)}\n");
            }
        }
    }
}
