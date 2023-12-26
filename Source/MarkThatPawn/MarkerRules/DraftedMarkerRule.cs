using Verse;

namespace MarkThatPawn.MarkerRules;

public class DraftedMarkerRule : MarkerRule
{
    public DraftedMarkerRule()
    {
        RuleType = AutoRuleType.Drafted;
        SetDefaultValues();
        IsOverride = true;
        ApplicablePawnTypes = [MarkThatPawn.PawnType.Colonist];
        if (!ModLister.IdeologyInstalled)
        {
            return;
        }

        ApplicablePawnTypes.Add(MarkThatPawn.PawnType.Slave);
        ApplicablePawnTypes.Add(MarkThatPawn.PawnType.Default);
    }

    public DraftedMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Drafted;
        SetBlob(blob);
        IsOverride = true;
        ApplicablePawnTypes = [MarkThatPawn.PawnType.Colonist];
        if (!ModLister.IdeologyInstalled)
        {
            return;
        }

        ApplicablePawnTypes.Add(MarkThatPawn.PawnType.Slave);
        ApplicablePawnTypes.Add(MarkThatPawn.PawnType.Default);
    }

    public override MarkerRule GetCopy()
    {
        return new DraftedMarkerRule(GetBlob());
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        if (pawn == null || pawn.Destroyed || !pawn.Spawned)
        {
            return false;
        }

        return pawn.Drafted;
    }
}