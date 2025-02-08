namespace MarkThatPawn.MarkerRules;

public class FactionIconMarkerRule : MarkerRule
{
    public FactionIconMarkerRule()
    {
        RuleType = AutoRuleType.FactionIcon;
        UsesDynamicIcons = true;
        SetDefaultValues();
    }

    public FactionIconMarkerRule(string blob)
    {
        RuleType = AutoRuleType.FactionIcon;
        UsesDynamicIcons = true;
        SetBlob(blob);
    }

    public override MarkerRule GetCopy()
    {
        return new FactionIconMarkerRule(GetBlob());
    }

    public override string GetMarkerBlob()
    {
        return "__custom__;FactionIcon";
    }
}