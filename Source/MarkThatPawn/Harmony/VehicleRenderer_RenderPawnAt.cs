using Verse;

namespace MarkThatPawn.Harmony;

public static class VehicleRenderer_RenderPawnAt
{
    public static void Postfix(Pawn ___vehicle)
    {
        if (!MarkThatPawn.ValidPawn(___vehicle))
        {
            return;
        }

        var tracker = ___vehicle.Map.GetComponent<MarkingTracker>();

        if (tracker == null)
        {
            return;
        }

        if (tracker.GlobalMarkingTracker.OverridePawns.TryGetValue(___vehicle, out _))
        {
            MarkThatPawn.RenderMarkingOverlay(___vehicle, -3, tracker);
            return;
        }

        var result = tracker.GlobalMarkingTracker.GetPawnMarking(___vehicle);
        if (result == 0)
        {
            return;
        }

        MarkThatPawn.RenderMarkingOverlay(___vehicle, result, tracker);
    }
}