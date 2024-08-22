using RimWorld;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class RelativeMarkerRule : MarkerRule
{
    public RelativeMarkerRule()
    {
        RuleType = AutoRuleType.Relative;
        SetDefaultValues();
    }

    public RelativeMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Relative;
        SetBlob(blob);
    }

    public override MarkerRule GetCopy()
    {
        return new RelativeMarkerRule(GetBlob());
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

        PawnRelationUtility.Notify_PawnsSeenByPlayer([pawn], out var pawnRelationsInfo, true, false);

        return !string.IsNullOrEmpty(pawnRelationsInfo);
    }
}