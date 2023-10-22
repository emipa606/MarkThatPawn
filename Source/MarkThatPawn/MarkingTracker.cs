using System.Collections.Generic;
using Verse;

namespace MarkThatPawn;

public class MarkingTracker : MapComponent
{
    public static MarkingTracker Instance;

    public Dictionary<Pawn, int> MarkedPawns = new Dictionary<Pawn, int>();
    private List<Pawn> markedPawnsKeys = new List<Pawn>();
    private List<int> markedPawnsValues = new List<int>();


    public MarkingTracker(Map map) : base(map)
    {
        Instance = this;
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        Instance = this;
    }

    public int GetPawnMarking(Pawn pawn)
    {
        return MarkedPawns.TryGetValue(pawn, out var result) ? result : 0;
    }

    public void SetPawnMarking(Pawn pawn, int mark, int currentMarking, MarkingTracker tracker,
        bool onlySelectedPawn = false)
    {
        if (onlySelectedPawn)
        {
            if (mark == 0)
            {
                if (MarkedPawns.ContainsKey(pawn))
                {
                    MarkedPawns.Remove(pawn);
                }

                return;
            }

            MarkedPawns[pawn] = mark;
            return;
        }

        var pawnSelector = MarkThatPawn.GetMarkerDefForPawn(pawn);
        foreach (var selectorSelectedObject in Find.Selector.SelectedObjects)
        {
            if (selectorSelectedObject is not Pawn selectedPawn)
            {
                continue;
            }

            if (selectedPawn != pawn)
            {
                if (MarkThatPawn.GetMarkerDefForPawn(selectedPawn) != pawnSelector)
                {
                    continue;
                }

                if (tracker.GetPawnMarking(selectedPawn) != currentMarking)
                {
                    continue;
                }
            }


            if (mark == 0)
            {
                if (MarkedPawns.ContainsKey(selectedPawn))
                {
                    MarkedPawns.Remove(selectedPawn);
                }

                continue;
            }

            MarkedPawns[selectedPawn] = mark;
        }
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (Find.TickManager.TicksGame % 250 != 0 || MarkedPawns.NullOrEmpty())
        {
            return;
        }

        MarkedPawns.RemoveAll(pair =>
            pair.Key == null || pair.Key.Destroyed);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref MarkedPawns, "MarkedPawns", LookMode.Reference, LookMode.Value, ref markedPawnsKeys,
            ref markedPawnsValues);
        Instance = this;
    }
}