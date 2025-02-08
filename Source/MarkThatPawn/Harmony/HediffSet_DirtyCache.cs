using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(HediffSet), nameof(HediffSet.DirtyCache))]
public static class HediffSet_DirtyCache
{
    public static void Postfix(Pawn ___pawn)
    {
        var globalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        globalMarkingTracker?.ThingsToEvaluate.Add(___pawn);

        var tracker = ___pawn.Map?.GetComponent<MarkingTracker>();
        if (tracker?.PawnsToEvaluate.Contains(___pawn) == true)
        {
            return;
        }

        tracker?.PawnsToEvaluate.Add(___pawn);
    }
}