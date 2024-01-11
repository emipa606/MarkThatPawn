using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class XenotypeMarkerRule : MarkerRule
{
    private XenotypeDef RuleXenotype;

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
        return base.CanEnable() && ModLister.BiotechInstalled && RuleXenotype != null;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var xenotypeArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(xenotypeArea, RuleXenotype?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showXenotypeSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(xenotypeArea, RuleXenotype?.LabelCap ?? "MTP.NoneSelected".Translate());
        }

        if (RuleXenotype == null)
        {
            return;
        }

        var xenotypeImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(xenotypeImageRect, RuleXenotype.description);
        GUI.DrawTexture(xenotypeImageRect, RuleXenotype.Icon);
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

        if (RuleParameters == null)
        {
            return;
        }

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        RuleXenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(RuleParameters);
        if (RuleXenotype != null)
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

        if (pawn == null || pawn.Destroyed || !pawn.Spawned)
        {
            return false;
        }

        return pawn.genes?.Xenotype == RuleXenotype;
    }


    private void showXenotypeSelectorMenu()
    {
        var xenotypeList = new List<FloatMenuOption>();

        foreach (var xenotype in MarkThatPawn.AllValidXenotypes)
        {
            xenotypeList.Add(new FloatMenuOption(xenotype.LabelCap, () =>
            {
                RuleParameters = xenotype.defName;
                RuleXenotype = xenotype;
            }, xenotype.Icon, Color.white));
        }

        Find.WindowStack.Add(new FloatMenu(xenotypeList));
    }
}