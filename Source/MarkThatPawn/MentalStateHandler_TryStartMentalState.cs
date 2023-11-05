using HarmonyLib;
using Verse;
using Verse.AI;

namespace MarkThatPawn;

[HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
public static class MentalStateHandler_TryStartMentalState
{
    public static void Postfix(Pawn ___pawn, MentalState ___curStateInt)
    {
        if (___curStateInt == null)
        {
            return;
        }

        var globalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        if (globalMarkingTracker == null)
        {
            return;
        }

        if (!MarkThatPawn.TryGetAutoMarkerForPawn(___pawn, out var result, typeof(MentalStateMarkerRule)))
        {
            return;
        }

        globalMarkingTracker.OverridePawns[___pawn] = $"{result}§MentalState";
        MarkThatPawn.ResetCache(___pawn);
    }
}