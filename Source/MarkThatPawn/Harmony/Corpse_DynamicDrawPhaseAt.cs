using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(Corpse), nameof(Corpse.DynamicDrawPhaseAt))]
public static class Corpse_DynamicDrawPhaseAt
{
    public static void Postfix(Corpse __instance)
    {
        if (!MarkThatPawnMod.instance.Settings.ShowOnCorpses)
        {
            return;
        }

        var pawn = __instance.InnerPawn;

        if (!MarkThatPawn.ValidPawn(pawn, true))
        {
            return;
        }

        var tracker = __instance.Map.GetComponent<MarkingTracker>();

        if (tracker == null)
        {
            return;
        }

        if (!tracker.GlobalMarkingTracker.HasAnyDefinedMarking(__instance))
        {
            return;
        }

        MarkThatPawn.RenderMarkingOverlay(__instance, tracker);
    }
}