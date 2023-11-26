using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MarkThatPawn;

public class MarkingTracker(Map map) : MapComponent(map)
{
    public readonly List<Pawn> PawnsToEvaluate = [];
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

        var firstPawn = PawnsToEvaluate.First();
        PawnsToEvaluate.Remove(firstPawn);

        if (!MarkThatPawn.TryGetAutoMarkerForPawn(firstPawn, out var result))
        {
            MarkThatPawn.ResetCache(firstPawn);
            return;
        }

        if (!GlobalMarkingTracker.HasAnyDefinedMarking(firstPawn))
        {
            GlobalMarkingTracker.MarkedPawns[firstPawn] = -1;
        }

        GlobalMarkingTracker.AutomaticPawns[firstPawn] = result;
        MarkThatPawn.ResetCache(firstPawn);
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

    private Dictionary<Pawn, string> AutomaticPawns = new Dictionary<Pawn, string>();
    private List<Pawn> automaticPawnsKeys = [];
    private List<string> automaticPawnsValues = [];
    private Dictionary<Pawn, string> CustomPawns = new Dictionary<Pawn, string>();
    private List<Pawn> customPawnsKeys = [];
    private List<string> customPawnsValues = [];
    private Dictionary<Pawn, int> MarkedPawns = new Dictionary<Pawn, int>();
    private List<Pawn> markedPawnsKeys = [];
    private List<int> markedPawnsValues = [];

    #endregion
}