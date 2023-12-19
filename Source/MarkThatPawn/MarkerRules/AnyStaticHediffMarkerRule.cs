using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class AnyStaticHediffMarkerRule : MarkerRule
{
    public AnyStaticHediffMarkerRule()
    {
        RuleType = AutoRuleType.AnyHediffStatic;
        SetDefaultValues();
    }

    public AnyStaticHediffMarkerRule(string blob)
    {
        RuleType = AutoRuleType.AnyHediffStatic;
        SetBlob(blob);
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
    }

    public override MarkerRule GetCopy()
    {
        return new AnyStaticHediffMarkerRule(GetBlob());
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

        var pawnHediffs = pawn.health?.hediffSet?.hediffs;

        return pawnHediffs != null && pawnHediffs.Any(hediff => hediff.IsPermanent() && hediff.def.isBad);
    }
}