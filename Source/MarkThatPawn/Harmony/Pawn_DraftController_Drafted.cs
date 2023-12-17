using HarmonyLib;
using RimWorld;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
public static class Pawn_DraftController_Drafted
{
    public static void Postfix(Pawn ___pawn)
    {
        var globalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        if (globalMarkingTracker?.PawnsToEvaluate != null && !globalMarkingTracker.PawnsToEvaluate.Contains(___pawn))
        {
            globalMarkingTracker.PawnsToEvaluate.Add(___pawn);
        }
    }
}