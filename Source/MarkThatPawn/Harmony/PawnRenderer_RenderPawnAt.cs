using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
public static class PawnRenderer_RenderPawnAt
{
    public static void Postfix(Pawn ___pawn)
    {
        ThingWithComps pawn = ___pawn;
        if (___pawn.Dead)
        {
            if (!MarkThatPawnMod.Instance.Settings.ShowOnCorpses)
            {
                return;
            }

            pawn = ___pawn.Corpse;
        }

        if (!MarkThatPawn.ValidPawn(___pawn, true))
        {
            return;
        }

        var map = pawn.Map ?? pawn.MapHeld;

        var tracker = map?.GetComponent<MarkingTracker>();

        if (tracker == null)
        {
            return;
        }

        if (!tracker.GlobalMarkingTracker.HasAnyDefinedMarking(pawn))
        {
            return;
        }

        MarkThatPawn.RenderMarkingOverlay(pawn, tracker);
    }
}