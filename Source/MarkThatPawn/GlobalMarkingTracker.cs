using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MarkThatPawn;

public class GlobalMarkingTracker : GameComponent
{
    public Dictionary<Pawn, string> AutomaticPawns = new Dictionary<Pawn, string>();
    private List<Pawn> automaticPawnsKeys = new List<Pawn>();
    private List<string> automaticPawnsValues = new List<string>();
    public Dictionary<Pawn, string> CustomPawns = new Dictionary<Pawn, string>();
    private List<Pawn> customPawnsKeys = new List<Pawn>();
    private List<string> customPawnsValues = new List<string>();
    public Dictionary<Pawn, int> MarkedPawns = new Dictionary<Pawn, int>();
    private List<Pawn> markedPawnsKeys = new List<Pawn>();
    private List<int> markedPawnsValues = new List<int>();
    public Dictionary<Pawn, string> OverridePawns = new Dictionary<Pawn, string>();
    private List<Pawn> overridePawnsKeys = new List<Pawn>();
    private List<string> overridePawnsValues = new List<string>();

    public GlobalMarkingTracker(Game game)
    {
    }

    public int GetPawnMarking(Pawn pawn)
    {
        return MarkedPawns.TryGetValue(pawn, out var result) ? result : 0;
    }

    public bool HasAnyDefinedMarking(Pawn pawn)
    {
        return MarkedPawns.ContainsKey(pawn) || CustomPawns.ContainsKey(pawn) || AutomaticPawns.ContainsKey(pawn);
    }

    public void SetPawnMarking(Pawn pawn, int mark, int currentMarking, bool onlySelectedPawn = false,
        string customMarkerString = null)
    {
        if (onlySelectedPawn)
        {
            if (CustomPawns.ContainsKey(pawn))
            {
                CustomPawns.Remove(pawn);
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

                if (GetPawnMarking(selectedPawn) != currentMarking)
                {
                    continue;
                }
            }

            if (CustomPawns.ContainsKey(selectedPawn))
            {
                CustomPawns.Remove(selectedPawn);
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

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref MarkedPawns, "MarkedPawns", LookMode.Reference, LookMode.Value, ref markedPawnsKeys,
            ref markedPawnsValues);
        Scribe_Collections.Look(ref AutomaticPawns, "AutomaticPawns", LookMode.Reference, LookMode.Value,
            ref automaticPawnsKeys, ref automaticPawnsValues);
        Scribe_Collections.Look(ref CustomPawns, "CustomPawns", LookMode.Reference, LookMode.Value,
            ref customPawnsKeys, ref customPawnsValues);
        Scribe_Collections.Look(ref OverridePawns, "OverridePawns", LookMode.Reference, LookMode.Value,
            ref overridePawnsKeys, ref overridePawnsValues);
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();

        if (Find.TickManager.TicksGame % GenDate.TicksPerDay != 0)
        {
            return;
        }

        MarkedPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        AutomaticPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        CustomPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        OverridePawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
    }
}