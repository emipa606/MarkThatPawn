using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace MarkThatPawn.Harmony;

[HarmonyPatch]
public static class OverrideTriggers
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(MentalStateHandler), "ClearMentalStateDirect");
        yield return AccessTools.Method(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState));
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