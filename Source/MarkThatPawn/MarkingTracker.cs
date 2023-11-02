using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MarkThatPawn;

public class MarkingTracker : MapComponent
{
    public readonly List<Pawn> PawnsToEvaluate = new List<Pawn>();
    public Dictionary<Pawn, string> AutomaticPawns = new Dictionary<Pawn, string>();
    private List<Pawn> automaticPawnsKeys = new List<Pawn>();
    private List<string> automaticPawnsValues = new List<string>();
    public Dictionary<Pawn, string> CustomPawns = new Dictionary<Pawn, string>();
    private List<Pawn> customPawnsKeys = new List<Pawn>();
    private List<string> customPawnsValues = new List<string>();
    public Dictionary<Pawn, int> MarkedPawns = new Dictionary<Pawn, int>();
    private List<Pawn> markedPawnsKeys = new List<Pawn>();
    private List<int> markedPawnsValues = new List<int>();


    public MarkingTracker(Map map) : base(map)
    {
    }

    public int GetPawnMarking(Pawn pawn)
    {
        return MarkedPawns.TryGetValue(pawn, out var result) ? result : 0;
    }

    public void SetPawnMarking(Pawn pawn, int mark, int currentMarking, MarkingTracker tracker,
        bool onlySelectedPawn = false, string customMarkerString = null)
    {
        if (onlySelectedPawn)
        {
            if (tracker.CustomPawns.ContainsKey(pawn))
            {
                tracker.CustomPawns.Remove(pawn);
            }

            if (mark == 0)
            {
                if (MarkedPawns.ContainsKey(pawn))
                {
                    MarkedPawns.Remove(pawn);
                }

                return;
            }

            if (customMarkerString != null)
            {
                CustomPawns[pawn] = customMarkerString;
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
                if (MarkThatPawn.GetMarkerDefForPawn(selectedPawn) != pawnSelector && mark > -2)
                {
                    continue;
                }

                if (tracker.GetPawnMarking(selectedPawn) != currentMarking)
                {
                    continue;
                }
            }

            if (tracker.CustomPawns.ContainsKey(selectedPawn))
            {
                tracker.CustomPawns.Remove(selectedPawn);
            }

            switch (mark)
            {
                case -2:
                case -1 when AutomaticPawns?.ContainsKey(selectedPawn) == true:
                case > 0:
                    MarkedPawns[selectedPawn] = mark;
                    if (customMarkerString != null)
                    {
                        CustomPawns[selectedPawn] = customMarkerString;
                    }

                    break;
                case 0:
                    if (MarkedPawns.ContainsKey(selectedPawn))
                    {
                        MarkedPawns.Remove(selectedPawn);
                    }

                    break;
            }
        }
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (PawnsToEvaluate?.Any() == true)
        {
            var firstPawn = PawnsToEvaluate.First();
            PawnsToEvaluate.Remove(firstPawn);
            if (MarkThatPawn.TryGetAutoMarkerForPawn(firstPawn, out var result))
            {
                AutomaticPawns[firstPawn] = result;
                MarkedPawns[firstPawn] = -1;
            }
        }

        if (Find.TickManager.TicksGame % GenDate.TicksPerDay != 0)
        {
            return;
        }

        MarkedPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        AutomaticPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        CustomPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref MarkedPawns, "MarkedPawns", LookMode.Reference, LookMode.Value, ref markedPawnsKeys,
            ref markedPawnsValues);
        Scribe_Collections.Look(ref AutomaticPawns, "AutomaticPawns", LookMode.Reference, LookMode.Value,
            ref automaticPawnsKeys, ref automaticPawnsValues);
        Scribe_Collections.Look(ref CustomPawns, "CustomPawns", LookMode.Reference, LookMode.Value,
            ref customPawnsKeys, ref customPawnsValues);
    }
}