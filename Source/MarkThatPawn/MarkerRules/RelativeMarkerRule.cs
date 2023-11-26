using RimWorld;
using UnityEngine;
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

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
    }

    public override MarkerRule GetCopy()
    {
        return new RelativeMarkerRule(GetBlob());
    }

    protected override void PopulateRuleParameterObjects()
    {
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

        PawnRelationUtility.Notify_PawnsSeenByPlayer(new[] { pawn }, out var pawnRelationsInfo, true, false);

        return !string.IsNullOrEmpty(pawnRelationsInfo);
    }
}