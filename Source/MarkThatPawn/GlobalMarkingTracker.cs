using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MarkThatPawn;

public class GlobalMarkingTracker : GameComponent
{
    public readonly List<ThingWithComps> ThingsToEvaluate = [];
    public Dictionary<ThingWithComps, string> AutomaticPawns = new();
    private List<ThingWithComps> automaticPawnsKeys = [];
    private List<string> automaticPawnsValues = [];
    public Dictionary<ThingWithComps, string> CustomPawns = new();
    private List<ThingWithComps> customPawnsKeys = [];
    private List<string> customPawnsValues = [];
    public Dictionary<ThingWithComps, int> MarkedPawns = new();
    private List<ThingWithComps> markedPawnsKeys = [];
    private List<int> markedPawnsValues = [];
    public Dictionary<ThingWithComps, string> OverridePawns = new();
    private List<ThingWithComps> overridePawnsKeys = [];
    private List<string> overridePawnsValues = [];

    public GlobalMarkingTracker(Game game)
    {
    }

    public int GetPawnMarking(ThingWithComps thing)
    {
        return MarkedPawns.GetValueOrDefault(thing, 0);
    }

    public void ReplacePawnWithCorpse(Pawn pawn, Corpse corpse)
    {
        if (AutomaticPawns.ContainsKey(pawn))
        {
            AutomaticPawns[corpse] = AutomaticPawns[pawn];
            AutomaticPawns.Remove(pawn);
        }

        if (ThingsToEvaluate.Contains(pawn))
        {
            ThingsToEvaluate.Add(corpse);
            ThingsToEvaluate.Remove(pawn);
        }

        if (CustomPawns.ContainsKey(pawn))
        {
            CustomPawns[corpse] = CustomPawns[pawn];
            CustomPawns.Remove(pawn);
        }

        if (MarkedPawns.ContainsKey(pawn))
        {
            MarkedPawns[corpse] = MarkedPawns[pawn];
            MarkedPawns.Remove(pawn);
        }

        if (!OverridePawns.ContainsKey(pawn))
        {
            return;
        }

        OverridePawns[corpse] = OverridePawns[pawn];
        OverridePawns.Remove(pawn);
    }

    public bool HasAnyDefinedMarking(ThingWithComps thing)
    {
        return MarkedPawns.ContainsKey(thing) || CustomPawns.ContainsKey(thing) || AutomaticPawns.ContainsKey(thing) ||
               OverridePawns.ContainsKey(thing);
    }

    public bool ShouldShowMultiMarking(ThingWithComps thing)
    {
        if (!MarkThatPawnMod.Instance.Settings.SeparateTemporary)
        {
            return false;
        }

        var currentMarking = GetPawnMarking(thing);
        return currentMarking != 0 && OverridePawns.ContainsKey(thing) ||
               OverridePawns.TryGetValue(thing, out var overrideValue) &&
               overrideValue.Split(MarkThatPawn.MarkerBlobSplitter).Length > 1 ||
               AutomaticPawns.TryGetValue(thing, out var automaticValue) &&
               automaticValue.Split(MarkThatPawn.MarkerBlobSplitter).Length > 1;
    }

    public void SetPawnMarking(ThingWithComps thing, int mark, int currentMarking, bool onlySelectedPawn = false,
        string customMarkerString = null)
    {
        if (onlySelectedPawn)
        {
            CustomPawns.Remove(thing);

            if (mark == 0)
            {
                MarkedPawns.Remove(thing);

                return;
            }

            if (customMarkerString != null)
            {
                CustomPawns[thing] = customMarkerString;
            }

            MarkedPawns[thing] = mark;
            return;
        }

        var pawnSelector = MarkThatPawn.GetMarkerDefForPawn(thing);
        foreach (var selectorSelectedObject in Find.Selector.SelectedObjects)
        {
            if (selectorSelectedObject is not Pawn and not Corpse)
            {
                continue;
            }

            var thingSelected = selectorSelectedObject as ThingWithComps;

            if (selectorSelectedObject != thing)
            {
                if (MarkThatPawn.GetMarkerDefForPawn(thingSelected) != pawnSelector && mark > -2)
                {
                    continue;
                }

                if (GetPawnMarking(thingSelected) != currentMarking)
                {
                    continue;
                }
            }

            CustomPawns.Remove(thingSelected);

            switch (mark)
            {
                case -2:
                case -1 when AutomaticPawns?.ContainsKey(thingSelected) == true:
                case > 0:
                    MarkedPawns[thingSelected] = mark;
                    if (customMarkerString != null)
                    {
                        CustomPawns[thingSelected] = customMarkerString;
                    }

                    break;
                case 0:
                    MarkedPawns.Remove(thingSelected);

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

        MarkedPawns ??= new Dictionary<ThingWithComps, int>();

        AutomaticPawns ??= new Dictionary<ThingWithComps, string>();

        CustomPawns ??= new Dictionary<ThingWithComps, string>();

        OverridePawns ??= new Dictionary<ThingWithComps, string>();
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

        if (ThingsToEvaluate?.Any() != true)
        {
            return;
        }

        var thing = ThingsToEvaluate.First();
        ThingsToEvaluate.Remove(thing);

        if (MarkThatPawnMod.Instance.Settings.AutoRules == null || !MarkThatPawnMod.Instance.Settings.AutoRules.Any())
        {
            return;
        }

        OverridePawns ??= [];

        if (thing is not Pawn pawn)
        {
            if (thing is not Corpse corpse)
            {
                return;
            }

            pawn = corpse.InnerPawn;
        }

        if (pawn == null || thing.Map == null)
        {
            return;
        }

        var mapTracker = thing.Map.GetComponent<MarkingTracker>();
        if (mapTracker?.PawnsToEvaluate != null && !mapTracker.PawnsToEvaluate.Contains(thing))
        {
            mapTracker.PawnsToEvaluate.Add(thing);
        }

        var overrideRules = MarkThatPawnMod.Instance.Settings.AutoRules
            .Where(rule => rule.Enabled && rule.IsOverride && rule.AppliesToPawn(pawn))
            .OrderBy(rule => rule.RuleOrder)
            .Select(markerRule => markerRule.GetMarkerBlob())
            .ToList();

        if (overrideRules.Any())
        {
            OverridePawns[thing] = string.Join(MarkThatPawn.MarkerBlobSplitter.ToString(), overrideRules);
        }
        else
        {
            OverridePawns.Remove(thing);
        }

        MarkThatPawn.ResetCache(thing);
    }
}