using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class MentalStateMarkerRule : MarkerRule
{
    private MentalStateType mentalStateType;

    public MentalStateMarkerRule()
    {
        RuleType = AutoRuleType.MentalState;
        mentalStateType = MentalStateType.Any;
        SetDefaultValues();
        RuleParameters = mentalStateType.ToString();
        IsOverride = true;
    }

    public MentalStateMarkerRule(string blob)
    {
        RuleType = AutoRuleType.MentalState;
        mentalStateType = MentalStateType.Any;
        SetBlob(blob);
        IsOverride = true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var mentalStateTypeArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(mentalStateTypeArea, $"MTP.MentalState.{mentalStateType}".Translate()))
            {
                showMentalTypeSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(mentalStateTypeArea, $"MTP.MentalState.{mentalStateType}".Translate());
        }
    }

    public override MarkerRule GetCopy()
    {
        return new MentalStateMarkerRule(GetBlob());
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

        if (Enum.TryParse(RuleParameters, out mentalStateType))
        {
            return;
        }

        ConfigError = true;
        ErrorMessage = $"Could not parse MentalStateType from {RuleParameters}, disabling rule";
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

        switch (mentalStateType)
        {
            case MentalStateType.Passive:
                return !pawn.Dead && pawn.mindState.mentalStateHandler.InMentalState &&
                       !pawn.mindState.mentalStateHandler.CurStateDef.IsAggro;
            case MentalStateType.Aggressive:
                return pawn.InAggroMentalState;
            default:
                return pawn.InMentalState;
        }
    }

    private void showMentalTypeSelectorMenu()
    {
        var mentalStateTypeMenu = new List<FloatMenuOption>();

        foreach (var stateType in (MentalStateType[])Enum.GetValues(typeof(MentalStateType)))
        {
            mentalStateTypeMenu.Add(new FloatMenuOption($"MTP.MentalState.{stateType}".Translate(), () =>
            {
                RuleParameters = stateType.ToString();
                mentalStateType = stateType;
            }));
        }

        Find.WindowStack.Add(new FloatMenu(mentalStateTypeMenu));
    }

    private enum MentalStateType
    {
        Any,
        Passive,
        Aggressive
    }
}