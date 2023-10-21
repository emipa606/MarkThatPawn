using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace MarkThatPawn;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
public static class Pawn_GetGizmos
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Pawn __instance)
    {
        if (values?.Any() == true)
        {
            foreach (var gizmo in values)
            {
                yield return gizmo;
            }
        }

        if (__instance is not { Spawned: true } || __instance.Map == null)
        {
            yield break;
        }

        var tracker = __instance.Map.GetComponent<MarkingTracker>();

        if (tracker == null)
        {
            yield break;
        }

        var currentMarking = tracker.GetPawnMarking(__instance);

        if (currentMarking > MarkThatPawn.GetTotalAmountOfMarkers())
        {
            Log.Warning(
                $"[MarkThatPawn]: {__instance.NameFullColored} had marker number {currentMarking} but there are only {MarkThatPawn.GetTotalAmountOfMarkers()} markers loaded. Removing marker.");
            tracker.SetPawnMarking(__instance, 0);
            currentMarking = tracker.GetPawnMarking(__instance);
        }

        var icon = MarkThatPawn.GetTextureForMarker(currentMarking);

        if (currentMarking != 0 && icon == null)
        {
            Log.Warning(
                $"[MarkThatPawn]: {__instance.NameFullColored} had marker number {currentMarking} but failed to find the texture. Removing marker.");
            tracker.SetPawnMarking(__instance, 0);
        }

        if (icon == null)
        {
            icon = MarkThatPawn.MarkerIcon;
        }

        yield return new Command_Action
        {
            defaultLabel = "MTP.SelectMarker".Translate(),
            icon = icon,
            defaultDesc = "MTP.SelectMarkerTT".Translate(),
            action = delegate
            {
                Find.WindowStack.Add(
                    new FloatMenu(MarkThatPawn.GetMarkingOptions(currentMarking, tracker, __instance)));
            }
        };
    }
}