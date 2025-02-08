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

    public override MarkerRule GetCopy()
    {
        return new AnyStaticHediffMarkerRule(GetBlob());
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        var pawnHediffs = pawn.health?.hediffSet?.hediffs;

        return pawnHediffs != null && pawnHediffs.Any(hediff => hediff.IsPermanent() && hediff.def.isBad);
    }
}