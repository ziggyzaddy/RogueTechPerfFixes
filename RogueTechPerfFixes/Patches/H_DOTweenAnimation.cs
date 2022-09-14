using System.Collections.Generic;
using DG.Tweening;
using Harmony;
using RogueTechPerfFixes.Utils;
using UnityEngine;

namespace RogueTechPerfFixes.Patches
{
    public static class H_DOTweenAnimation
    {
        private static readonly Dictionary<object, List<DOTweenAnimation>> _tweenTable = new Dictionary<object, List<DOTweenAnimation>>();

        [HarmonyPatch(typeof(DOTweenAnimation), nameof(DOTweenAnimation.DOKill))]
        public static class H_DoKill
        {
            public static bool Prepare()
            {
                return Mod.Settings.Patch.Vanilla;
            }

            public static bool Prefix(DOTweenAnimation __instance)
            {
                if (!__instance.tween?.HasTargetId ?? true)
                    return true;

                DOTween.Kill(__instance.tween.TargetId);
                __instance.tween = null;
                return false;
            }
        }

        [HarmonyPatch(typeof(DOTweenAnimation), nameof(DOTweenAnimation.CreateTween))]
        public static class H_CreateTween
        {
            public static bool Prepare()
            {
                return Mod.Settings.Patch.Vanilla;
            }

            public static void Postfix(DOTweenAnimation __instance)
            {
                var tween = __instance.tween;
                if (tween is null)
                    return;

                var gameObject = __instance.gameObject;
                tween.TargetId = gameObject.GetInstanceID();
                tween.HasTargetId = true;
                RTPFLogger.Debug?.Write($"Target instance Id: {tween.TargetId}");
            }
        }
    }
}
