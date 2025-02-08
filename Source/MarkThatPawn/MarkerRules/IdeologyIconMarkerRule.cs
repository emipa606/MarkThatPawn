using Verse;

namespace MarkThatPawn.MarkerRules;

public class IdeologyIconMarkerRule : MarkerRule
{
    public IdeologyIconMarkerRule()
    {
        RuleType = AutoRuleType.IdeologyIcon;
        UsesDynamicIcons = true;
        SetDefaultValues();
    }

    public IdeologyIconMarkerRule(string blob)
    {
        RuleType = AutoRuleType.IdeologyIcon;
        UsesDynamicIcons = true;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && ModLister.IdeologyInstalled;
    }

    public override void PopulateRuleParameterObjects()
    {
        if (ModLister.IdeologyInstalled)
        {
            return;
        }

        ErrorMessage = "Ideology missing, disabling rule";
        ConfigError = true;
    }

    public override MarkerRule GetCopy()
    {
        return new IdeologyIconMarkerRule(GetBlob());
    }

    public override string GetMarkerBlob()
    {
        return "__custom__;IdeologyIcon";
    }
}