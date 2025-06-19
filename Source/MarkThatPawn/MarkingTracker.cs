using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MarkThatPawn;

public class MarkingTracker(Map map) : MapComponent(map)
{
    public readonly List<ThingWithComps> PawnsToEvaluate = [];
    public GlobalMarkingTracker GlobalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();

    public override void MapComponentTick()
    {
        base.MapComponentTick();

        if (GlobalMarkingTracker == null)
        {
            GlobalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        }

        if (PawnsToEvaluate?.Any() != true)
        {
            return;
        }

        var firstThing = PawnsToEvaluate.First();
        PawnsToEvaluate.Remove(firstThing);

        if (!MarkThatPawn.TryGetAutoMarkerForPawn(firstThing, out var result))
        {
            GlobalMarkingTracker.AutomaticPawns.Remove(firstThing);

            if (GlobalMarkingTracker.MarkedPawns.TryGetValue(firstThing, out var currentValue) && currentValue == -1)
            {
                GlobalMarkingTracker.MarkedPawns[firstThing] = 0;
            }

            MarkThatPawn.ResetCache(firstThing);
            return;
        }

        if (!GlobalMarkingTracker.HasAnyDefinedMarking(firstThing) ||
            GlobalMarkingTracker.MarkedPawns.TryGetValue(firstThing, out var currentMarkValue) && currentMarkValue == 0)
        {
            GlobalMarkingTracker.MarkedPawns[firstThing] = -1;
        }

        GlobalMarkingTracker.AutomaticPawns[firstThing] = result;
        MarkThatPawn.ResetCache(firstThing);
    }


    #region Legacy code used before the move to game-component

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref MarkedPawns, "MarkedPawns", LookMode.Reference, LookMode.Value, ref markedPawnsKeys,
            ref markedPawnsValues);
        Scribe_Collections.Look(ref AutomaticPawns, "AutomaticPawns", LookMode.Reference, LookMode.Value,
            ref automaticPawnsKeys, ref automaticPawnsValues);
        Scribe_Collections.Look(ref CustomPawns, "CustomPawns", LookMode.Reference, LookMode.Value,
            ref customPawnsKeys, ref customPawnsValues);
        if (Scribe.mode != LoadSaveMode.PostLoadInit ||
            !MarkedPawns.Any() && !AutomaticPawns.Any() && !CustomPawns.Any())
        {
            return;
        }

        Log.Message(
            $"[MarkThatPawn]: Found map-specific marking-values for map {map}, converting to global. This should only happen once per map.");
        if (GlobalMarkingTracker == null)
        {
            GlobalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        }

        if (MarkedPawns.Any())
        {
            foreach (var pawnsKey in MarkedPawns.Keys)
            {
                if (GlobalMarkingTracker.MarkedPawns.ContainsKey(pawnsKey))
                {
                    continue;
                }

                GlobalMarkingTracker.MarkedPawns[pawnsKey] = MarkedPawns[pawnsKey];
            }

            MarkedPawns.Clear();
        }

        if (AutomaticPawns.Any())
        {
            foreach (var pawnsKey in AutomaticPawns.Keys)
            {
                if (GlobalMarkingTracker.AutomaticPawns.ContainsKey(pawnsKey))
                {
                    continue;
                }

                GlobalMarkingTracker.AutomaticPawns[pawnsKey] = AutomaticPawns[pawnsKey];
            }

            AutomaticPawns.Clear();
        }

        if (!CustomPawns.Any())
        {
            return;
        }

        foreach (var pawnsKey in CustomPawns.Keys)
        {
            if (GlobalMarkingTracker.CustomPawns.ContainsKey(pawnsKey))
            {
                continue;
            }

            GlobalMarkingTracker.CustomPawns[pawnsKey] = CustomPawns[pawnsKey];
        }

        GlobalMarkingTracker.CustomPawns = CustomPawns.ToDictionary(pair => pair.Key, pair => pair.Value);
        CustomPawns.Clear();
    }

    private Dictionary<ThingWithComps, string> AutomaticPawns = new();
    private List<ThingWithComps> automaticPawnsKeys = [];
    private List<string> automaticPawnsValues = [];
    private Dictionary<ThingWithComps, string> CustomPawns = new();
    private List<ThingWithComps> customPawnsKeys = [];
    private List<string> customPawnsValues = [];
    private Dictionary<ThingWithComps, int> MarkedPawns = new();
    private List<ThingWithComps> markedPawnsKeys = [];
    private List<int> markedPawnsValues = [];

    #endregion
}