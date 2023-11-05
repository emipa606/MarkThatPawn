using HarmonyLib;
using RimWorld;
using Verse;

namespace MarkThatPawn;

[HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
public static class Pawn_DraftController_Drafted
{
    public static void Prefix(out bool __state, bool ___draftedInt)
    {
        __state = ___draftedInt;
    }

    public static void Postfix(Pawn ___pawn, bool ___draftedInt, bool __state)
    {
        if (___draftedInt == __state)
        {
            return;
        }

        var globalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        if (globalMarkingTracker == null)
        {
            return;
        }

        if (___draftedInt)
        {
            if (!MarkThatPawn.TryGetAutoMarkerForPawn(___pawn, out var result, typeof(DraftedMarkerRule)))
            {
                return;
            }

            globalMarkingTracker.OverridePawns[___pawn] = $"{result}§Drafted";
            MarkThatPawn.ResetCache(___pawn);
            return;
        }

        if (!globalMarkingTracker.OverridePawns.TryGetValue(___pawn, out var overrideString))
        {
            return;
        }

        if (!overrideString.EndsWith("§Drafted"))
        {
            return;
        }

        globalMarkingTracker.OverridePawns.Remove(___pawn);
        MarkThatPawn.ResetCache(___pawn);
    }
}