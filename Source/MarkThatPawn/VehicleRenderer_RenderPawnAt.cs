using Verse;

namespace MarkThatPawn;

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

        var result = tracker.GetPawnMarking(___vehicle);
        if (result == 0)
        {
            return;
        }

        MarkThatPawn.RenderMarkingOverlay(___vehicle, result);
    }
}