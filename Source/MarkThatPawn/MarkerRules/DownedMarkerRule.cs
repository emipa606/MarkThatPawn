using Verse;

namespace MarkThatPawn.MarkerRules;

public class DownedMarkerRule : MarkerRule
{
    public DownedMarkerRule()
    {
        RuleType = AutoRuleType.Downed;
        SetDefaultValues();
        IsOverride = true;
    }

    public DownedMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Downed;
        SetBlob(blob);
        IsOverride = true;
    }

    public override MarkerRule GetCopy()
    {
        return new DownedMarkerRule(GetBlob());
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

        return pawn.Downed;
    }
}