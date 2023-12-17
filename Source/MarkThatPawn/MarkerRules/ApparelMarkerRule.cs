using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class ApparelMarkerRule : MarkerRule
{
    private ThingDef ApparelThingDef;

    public ApparelMarkerRule()
    {
        RuleType = AutoRuleType.Apparel;
        SetDefaultValues();
    }

    public ApparelMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Apparel;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && ApparelThingDef != null;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var ApparelArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(ApparelArea, ApparelThingDef?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showApparelSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(ApparelArea, ApparelThingDef?.LabelCap ?? "MTP.NoneSelected".Translate());
        }

        if (ApparelThingDef == null)
        {
            return;
        }

        var ApparelImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(ApparelImageRect, ApparelThingDef.description);
        GUI.DrawTexture(ApparelImageRect, Widgets.GetIconFor(ApparelThingDef));
    }

    public override MarkerRule GetCopy()
    {
        return new ApparelMarkerRule(GetBlob());
    }

    protected override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        ApparelThingDef = DefDatabase<ThingDef>.GetNamedSilentFail(RuleParameters);
        if (ApparelThingDef != null)
        {
            return;
        }

        ErrorMessage = $"Could not find Apparel with defname {RuleParameters}, disabling rule";
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

        return pawn.apparel is { AnyApparel: true } &&
               pawn.apparel.WornApparel.Any(thing => thing.def == ApparelThingDef);
    }


    private void showApparelSelectorMenu()
    {
        var ApparelList = new List<FloatMenuOption>();

        foreach (var Apparel in MarkThatPawn.AllValidApparels)
        {
            ApparelList.Add(new FloatMenuOption(Apparel.LabelCap, () =>
            {
                RuleParameters = Apparel.defName;
                ApparelThingDef = Apparel;
            }, Apparel));
        }

        Find.WindowStack.Add(new FloatMenu(ApparelList));
    }
}