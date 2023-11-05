using HarmonyLib;
using Verse;
using Verse.AI;

namespace MarkThatPawn;

[HarmonyPatch(typeof(MentalStateHandler), "ClearMentalStateDirect")]
public static class MentalStateHandler_ClearMentalStateDirect
{
    public static void Postfix(Pawn ___pawn)
    {
        var globalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        if (globalMarkingTracker == null)
        {
            return;
        }

        if (!globalMarkingTracker.OverridePawns.TryGetValue(___pawn, out var overrideString))
        {
            return;
        }

        if (!overrideString.EndsWith("§MentalState"))
        {
            return;
        }

        globalMarkingTracker.OverridePawns.Remove(___pawn);
        MarkThatPawn.ResetCache(___pawn);
    }
}