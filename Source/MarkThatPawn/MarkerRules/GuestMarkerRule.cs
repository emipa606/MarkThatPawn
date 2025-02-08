using RimWorld;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class GuestMarkerRule : MarkerRule
{
    public GuestMarkerRule()
    {
        RuleType = AutoRuleType.Guest;
        SetDefaultValues();
        ApplicablePawnTypes =
        [
            MarkThatPawn.PawnType.Neutral
        ];
    }

    public GuestMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Guest;
        SetBlob(blob);
        ApplicablePawnTypes =
        [
            MarkThatPawn.PawnType.Neutral
        ];
    }

    public override MarkerRule GetCopy()
    {
        return new GuestMarkerRule(GetBlob());
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        return pawn.GuestStatus == GuestStatus.Guest || pawn.IsQuestLodger();
    }
}