using BattleTech;
using RogueTechPerfFixes.Models;

namespace RogueTechPerfFixes.Patches;

public static class H_ActorMovementSequence
{
    private static bool _hasEntered = false;

    private static int _counter = 0;

    [HarmonyPatch(typeof(ActorMovementSequence), nameof(ActorMovementSequence.Update))]
    public static class H_Update
    {
        public static bool Prepare()
        {
            return Mod.Settings.Patch.LowVisibility;
        }

        public static void Prefix()
        {
            if (!_hasEntered)
            {
                _hasEntered = true;
                VisibilityCacheGate.EnterGate();
                _counter = VisibilityCacheGate.GetCounter;
                Log.Main.Debug?.Log($"Enter visibility cache gate in {typeof(H_Update).FullName}:{nameof(Prefix)}\n");
            }
        }
    }

    [HarmonyPatch(typeof(ActorMovementSequence), "CompleteMove")]
    public static class H_CompleteMove
    {
        public static bool Prepare()
        {
            return Mod.Settings.Patch.LowVisibility;
        }

        public static void Postfix()
        {
            _hasEntered = false;
            VisibilityCacheGate.ExitGate();

            Utils.Utils.CheckExitCounter($"Fewer calls made to ExitGate() when reaches ActorMovementSequence.CompleteMove().\n", _counter);
            Log.Main.Debug?.Log($"Exit visibility cache gate in {typeof(H_CompleteMove).FullName}: {nameof(Postfix)}\n");
        }
    }
}