using System.Reflection;
using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch]
public static class Pawn_HealthTracker_Downstates
{
    public static MethodInfo[] TargetMethods()
    {
        return
        [
            AccessTools.Method(typeof(Pawn_HealthTracker), "MakeDowned"),
            AccessTools.Method(typeof(Pawn_HealthTracker), "MakeUndowned")
        ];
    }

    public static void Postfix(Pawn ___pawn)
    {
        var globalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        if (globalMarkingTracker?.ThingsToEvaluate != null && !globalMarkingTracker.ThingsToEvaluate.Contains(___pawn))
        {
            globalMarkingTracker.ThingsToEvaluate.Add(___pawn);
        }
    }
}