using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class MechanoidMarkerRule : MarkerRule
{
    private ThingDef mechanoid;

    public MechanoidMarkerRule()
    {
        RuleType = AutoRuleType.Mechanoid;
        SetDefaultValues();
        ApplicablePawnTypes =
        [
            MarkThatPawn.PawnType.Colonist,
            MarkThatPawn.PawnType.Enemy,
            MarkThatPawn.PawnType.Neutral
        ];
    }

    public MechanoidMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Mechanoid;
        SetBlob(blob);
        ApplicablePawnTypes =
        [
            MarkThatPawn.PawnType.Colonist,
            MarkThatPawn.PawnType.Enemy,
            MarkThatPawn.PawnType.Neutral
        ];
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var mechanoidArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(mechanoidArea,
                    mechanoid?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showMechanoidSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(mechanoidArea,
                mechanoid?.LabelCap ?? "MTP.NoneSelected".Translate());
        }

        if (mechanoid == null)
        {
            return;
        }

        var mechanoidImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(mechanoidImageRect, mechanoid.description);
        GUI.DrawTexture(mechanoidImageRect, Widgets.GetIconFor(mechanoid));
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && mechanoid != null;
    }

    public override MarkerRule GetCopy()
    {
        return new MechanoidMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        mechanoid = DefDatabase<ThingDef>.GetNamedSilentFail(RuleParameters);
        if (mechanoid != null)
        {
            return;
        }

        ErrorMessage = $"Could not find mechanoid with defname {RuleParameters}, disabling rule";
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

        return pawn.def == mechanoid;
    }

    private void showMechanoidSelectorMenu()
    {
        var mechanoidList = new List<FloatMenuOption>();

        foreach (var mechanoidToSelect in MarkThatPawn.AllMechanoids)
        {
            mechanoidList.Add(new FloatMenuOption(mechanoidToSelect.LabelCap, () =>
            {
                RuleParameters = mechanoidToSelect.defName;
                mechanoid = mechanoidToSelect;
            }, mechanoidToSelect));
        }

        Find.WindowStack.Add(new FloatMenu(mechanoidList));
    }
}