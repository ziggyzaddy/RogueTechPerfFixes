using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace RogueTechPerfFixes.Models
{
    /// <summary>
    /// Pool all requests to rebuild the visibility cache into one place.
    /// </summary>
    public class VisibilityCacheGate : ActionSemaphore
    {
        private static VisibilityCacheGate cacheGate = new VisibilityCacheGate();

        private readonly HashSet<AbstractActor> selfCacheActors = new HashSet<AbstractActor>();

        private readonly HashSet<AbstractActor> biCacheActors = new HashSet<AbstractActor>();

        private delegate void CheckForAlertDelegate(VisibilityCache cache);

        private VisibilityCacheGate()
            : base(null, null)
        {
            shouldTakeaction = () => counter == 0;
            actionToTake = () =>
            {
                var combatGameState = UnityGameInstance.BattleTechGame.Combat;
                var combatants = combatGameState.GetAllLivingCombatants();

                foreach (var actor in selfCacheActors.ToList())
                {
                    if (biCacheActors.Contains(actor))
                    {
                        selfCacheActors.Remove(actor);
                    }
                }

                foreach (var actor in selfCacheActors)
                {
                    actor.RebuildVisibilityCache(combatants);
                }

                foreach (var actor in biCacheActors)
                {
                    actor.UpdateVisibilityCache(combatants);
                }

                selfCacheActors.Clear();
                biCacheActors.Clear();
            };
        }

        public static bool Active => cacheGate.counter > 0;

        public static int GetCounter => cacheGate.counter;

        public static void EnterGate()
        {
            cacheGate.Enter();
        }

        public static void ExitGate()
        {
            cacheGate.Exit();
        }

        public static void ExitAll()
        {
            cacheGate.ResetHard();
        }

        public static void Reset()
        {
            cacheGate.ResetSemaphore();
        }

        public static void AddActorToRefresh(AbstractActor actor)
        {
            cacheGate.selfCacheActors.Add(actor);
        }

        public static void AddActorToRefreshReciprocal(AbstractActor actor)
        {
            cacheGate.biCacheActors.Add(actor);
        }

        #region Overrides of ActionSemaphore

        public override void ResetSemaphore()
        {
            base.ResetSemaphore();
            selfCacheActors.Clear();
        }

        #endregion

        private static void RebuildSharedCache(List<SharedVisibilityCache> list, List<ICombatant> combatatns)
        {
            for (var j = 0; j < list.Count; j++)
            {
                list[j].RebuildCache(combatatns);
                if (list[j].ReportVisibilityToParent)
                {
                    list.Add(list[j].ParentCache);
                }
            }
        }
    }
}
