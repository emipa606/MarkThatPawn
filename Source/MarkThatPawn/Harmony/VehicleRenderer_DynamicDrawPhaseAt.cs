using Verse;

namespace MarkThatPawn.Harmony;

public static class VehicleRenderer_DynamicDrawPhaseAt
{
    public static void Postfix(Pawn ___vehicle)
    {
        if (!MarkThatPawn.ValidPawn(___vehicle, true))
        {
            return;
        }

        var map = ___vehicle.Map ?? ___vehicle.MapHeld;

        var tracker = map?.GetComponent<MarkingTracker>();

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