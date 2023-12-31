using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch]
public static class Pawn_GetGizmos
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos));

        if (ModLister.GetActiveModWithIdentifier("SmashPhil.VehicleFramework") != null)
        {
            yield return AccessTools.Method("Vehicles.VehiclePawn:GetGizmos");
        }
    }


    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Pawn __instance)
    {
        if (values?.Any() == true)
        {
            foreach (var gizmo in values)
            {
                yield return gizmo;
            }
        }

        if (!MarkThatPawn.ValidPawn(__instance))
        {
            yield break;
        }

        var tracker = __instance.Map.GetComponent<MarkingTracker>();

        if (tracker == null)
        {
            yield break;
        }

        var currentMarking = tracker.GlobalMarkingTracker.GetPawnMarking(__instance);
        var currentMarkerSet = MarkThatPawn.GetMarkerDefForPawn(__instance);

        if (currentMarking > currentMarkerSet.MarkerTextures.Count)
        {
            Log.Warning(
                $"[MarkThatPawn]: {__instance.NameFullColored} had marker number {currentMarking} but there are only {currentMarkerSet.MarkerTextures.Count} markers loaded. Removing marker.");
            tracker.GlobalMarkingTracker.SetPawnMarking(__instance, 0, currentMarking, true);
            currentMarking = tracker.GlobalMarkingTracker.GetPawnMarking(__instance);
        }

        var icon = MarkThatPawn.MarkerIcon;

        switch (currentMarking)
        {
            case -2:
                if (!tracker.GlobalMarkingTracker.CustomPawns.TryGetValue(__instance, out var customString) ||
                    !MarkThatPawn.TryToConvertStringToTexture2D(customString, out icon))
                {
                    icon = BaseContent.BadTex;
                }

                break;
            case -1:
                if (!tracker.GlobalMarkingTracker.AutomaticPawns.TryGetValue(__instance, out var autoString))
                {
                    icon = BaseContent.BadTex;
                    break;
                }

                var firstAutoString = autoString.Split(MarkThatPawn.MarkerBlobSplitter)[0];
                if (!MarkThatPawn.TryToConvertStringToTexture2D(firstAutoString, out icon, __instance))
                {
                    icon = BaseContent.BadTex;
                }

                break;
            case > 0:
                icon = currentMarkerSet.MarkerTextures[currentMarking - 1];
                break;
        }

        yield return new Command_Action
        {
            defaultLabel = "MTP.SelectMarker".Translate(),
            icon = icon,
            defaultDesc = "MTP.SelectMarkerTT".Translate(),
            action = delegate
            {
                Find.WindowStack.Add(
                    new FloatMenu(MarkThatPawn.GetMarkingOptions(currentMarking, tracker, currentMarkerSet,
                        __instance)));
            }
        };
    }
}