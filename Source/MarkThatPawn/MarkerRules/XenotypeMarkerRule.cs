using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class XenotypeMarkerRule : MarkerRule
{
    private XenotypeDef ruleXenotype;

    public XenotypeMarkerRule()
    {
        RuleType = AutoRuleType.Xenotype;
        SetDefaultValues();
    }

    public XenotypeMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Xenotype;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && ModLister.BiotechInstalled && ruleXenotype != null;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var xenotypeArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(xenotypeArea, ruleXenotype?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showXenotypeSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(xenotypeArea, ruleXenotype?.LabelCap ?? "MTP.NoneSelected".Translate());
        }

        if (ruleXenotype == null)
        {
            return;
        }

        var xenotypeImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(xenotypeImageRect, ruleXenotype.description);
        GUI.DrawTexture(xenotypeImageRect, ruleXenotype.Icon);
    }

    public override MarkerRule GetCopy()
    {
        return new XenotypeMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        if (!ModLister.BiotechInstalled)
        {
            ErrorMessage = "Biotech missing, disabling rule";
            ConfigError = true;
            return;
        }

        switch (RuleParameters)
        {
            case null:
            case "" when !Enabled:
                return;
        }

        ruleXenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(RuleParameters);
        if (ruleXenotype != null)
        {
            return;
        }

        ErrorMessage = $"Could not find Xenotype with defname {RuleParameters}, disabling rule";
        ConfigError = true;
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        return pawn.genes?.Xenotype == ruleXenotype;
    }


    private void showXenotypeSelectorMenu()
    {
        var xenotypeList = new List<FloatMenuOption>();

        foreach (var xenotype in MarkThatPawn.AllValidXenotypes)
        {
            xenotypeList.Add(new FloatMenuOption(xenotype.LabelCap, () =>
            {
                RuleParameters = xenotype.defName;
                ruleXenotype = xenotype;
            }, xenotype.Icon, Color.white));
        }

        Find.WindowStack.Add(new FloatMenu(xenotypeList));
    }
}