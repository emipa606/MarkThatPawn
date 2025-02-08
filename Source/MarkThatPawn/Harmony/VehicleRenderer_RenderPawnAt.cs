using Verse;

namespace MarkThatPawn.Harmony;

public static class VehicleRenderer_RenderPawnAt
{
    public static void Postfix(Pawn ___vehicle)
    {
        if (!MarkThatPawn.ValidPawn(___vehicle, true))
        {
            return;
        }

        var tracker = ___vehicle.Map.GetComponent<MarkingTracker>();

        if (tracker == null)
        {
            return;
        }

        if (!tracker.GlobalMarkingTracker.HasAnyDefinedMarking(___vehicle))
        {
            return;
        }

        MarkThatPawn.RenderMarkingOverlay(___vehicle, tracker);
    }
}