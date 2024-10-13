using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MarkThatPawn;

public class GlobalMarkingTracker : GameComponent
{
    public readonly List<Pawn> PawnsToEvaluate = [];
    public Dictionary<Pawn, string> AutomaticPawns = new Dictionary<Pawn, string>();
    private List<Pawn> automaticPawnsKeys = [];
    private List<string> automaticPawnsValues = [];
    public Dictionary<Pawn, string> CustomPawns = new Dictionary<Pawn, string>();
    private List<Pawn> customPawnsKeys = [];
    private List<string> customPawnsValues = [];
    public Dictionary<Pawn, int> MarkedPawns = new Dictionary<Pawn, int>();
    private List<Pawn> markedPawnsKeys = [];
    private List<int> markedPawnsValues = [];
    public Dictionary<Pawn, string> OverridePawns = new Dictionary<Pawn, string>();
    private List<Pawn> overridePawnsKeys = [];
    private List<string> overridePawnsValues = [];

    public GlobalMarkingTracker(Game game)
    {
    }

    public int GetPawnMarking(Pawn pawn)
    {
        return MarkedPawns.GetValueOrDefault(pawn, 0);
    }

    public bool HasAnyDefinedMarking(Pawn pawn)
    {
        return MarkedPawns.ContainsKey(pawn) || CustomPawns.ContainsKey(pawn) || AutomaticPawns.ContainsKey(pawn) ||
               OverridePawns.ContainsKey(pawn);
    }

    public bool ShouldShowMultiMarking(Pawn pawn)
    {
        if (!MarkThatPawnMod.instance.Settings.SeparateTemporary)
        {
            return false;
        }

        var currentMarking = GetPawnMarking(pawn);
        return currentMarking != 0 && OverridePawns.ContainsKey(pawn) ||
               OverridePawns.TryGetValue(pawn, out var overrideValue) &&
               overrideValue.Split(MarkThatPawn.MarkerBlobSplitter).Length > 1 ||
               AutomaticPawns.TryGetValue(pawn, out var automaticValue) &&
               automaticValue.Split(MarkThatPawn.MarkerBlobSplitter).Length > 1;
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
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            // Remove all null-keys before saving
            MarkedPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
            AutomaticPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
            CustomPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
            OverridePawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        }

        Scribe_Collections.Look(ref MarkedPawns, "MarkedPawns", LookMode.Reference, LookMode.Value, ref markedPawnsKeys,
            ref markedPawnsValues);
        Scribe_Collections.Look(ref AutomaticPawns, "AutomaticPawns", LookMode.Reference, LookMode.Value,
            ref automaticPawnsKeys, ref automaticPawnsValues);
        Scribe_Collections.Look(ref CustomPawns, "CustomPawns", LookMode.Reference, LookMode.Value,
            ref customPawnsKeys, ref customPawnsValues);
        Scribe_Collections.Look(ref OverridePawns, "OverridePawns", LookMode.Reference, LookMode.Value,
            ref overridePawnsKeys, ref overridePawnsValues);

        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        if (MarkedPawns == null)
        {
            MarkedPawns = new Dictionary<Pawn, int>();
        }

        if (AutomaticPawns == null)
        {
            AutomaticPawns = new Dictionary<Pawn, string>();
        }

        if (CustomPawns == null)
        {
            CustomPawns = new Dictionary<Pawn, string>();
        }

        if (OverridePawns == null)
        {
            OverridePawns = new Dictionary<Pawn, string>();
        }
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();

        if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
        {
            MarkedPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
            AutomaticPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
            CustomPawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
            OverridePawns?.RemoveAll(pair => pair.Key == null || pair.Key.Destroyed);
        }

        if (PawnsToEvaluate?.Any() != true)
        {
            return;
        }

        var firstPawn = PawnsToEvaluate.First();
        PawnsToEvaluate.Remove(firstPawn);

        if (MarkThatPawnMod.instance.Settings.AutoRules == null || !MarkThatPawnMod.instance.Settings.AutoRules.Any())
        {
            return;
        }

        if (OverridePawns == null)
        {
            OverridePawns = [];
        }

        var mapTracker = firstPawn.Map?.GetComponent<MarkingTracker>();
        if (mapTracker?.PawnsToEvaluate != null && !mapTracker.PawnsToEvaluate.Contains(firstPawn))
        {
            mapTracker.PawnsToEvaluate.Add(firstPawn);
        }

        var overrideRules = new List<string>();
        foreach (var markerRule in MarkThatPawnMod.instance.Settings.AutoRules
                     .Where(rule => rule.Enabled && rule.IsOverride && rule.AppliesToPawn(firstPawn))
                     .OrderBy(rule => rule.RuleOrder))
        {
            overrideRules.Add(markerRule.GetMarkerBlob());
        }

        if (overrideRules.Any())
        {
            OverridePawns[firstPawn] = string.Join(MarkThatPawn.MarkerBlobSplitter.ToString(), overrideRules);
        }
        else
        {
            if (OverridePawns.ContainsKey(firstPawn))
            {
                OverridePawns.Remove(firstPawn);
            }
        }

        MarkThatPawn.ResetCache(firstPawn);
    }
}