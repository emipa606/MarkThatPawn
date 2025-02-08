using Verse;

namespace MarkThatPawn.MarkerRules;

public class FactionLeaderMarkerRule : MarkerRule
{
    public FactionLeaderMarkerRule()
    {
        RuleType = AutoRuleType.FactionLeader;
        SetDefaultValues();
    }

    public FactionLeaderMarkerRule(string blob)
    {
        RuleType = AutoRuleType.FactionLeader;
        SetBlob(blob);
    }

    public override MarkerRule GetCopy()
    {
        return new FactionLeaderMarkerRule(GetBlob());
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        if (pawn.Faction == null)
        {
            return false;
        }

        return pawn == pawn.Faction.leader;
    }
}