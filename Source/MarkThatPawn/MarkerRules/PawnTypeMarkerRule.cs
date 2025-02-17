using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class PawnTypeMarkerRule : MarkerRule
{
    private MarkThatPawn.PawnType pawnType;

    public PawnTypeMarkerRule()
    {
        RuleType = AutoRuleType.PawnType;
        pawnType = MarkThatPawn.PawnType.Default;
        SetDefaultValues();
        ApplicablePawnTypes = [MarkThatPawn.PawnType.Default];
    }

    public PawnTypeMarkerRule(string blob)
    {
        RuleType = AutoRuleType.PawnType;
        pawnType = MarkThatPawn.PawnType.Default;
        SetBlob(blob);
        ApplicablePawnTypes = [MarkThatPawn.PawnType.Default];
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var pawnTypeArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(pawnTypeArea,
                    pawnType == MarkThatPawn.PawnType.Default
                        ? "MTP.NoneSelected".Translate()
                        : $"MTP.PawnType.{pawnType}".Translate()))
            {
                showPawnTypeSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(pawnTypeArea,
                pawnType == MarkThatPawn.PawnType.Default
                    ? "MTP.NoneSelected".Translate()
                    : $"MTP.PawnType.{pawnType}".Translate());
        }
    }

    public override MarkerRule GetCopy()
    {
        return new PawnTypeMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        if (Enum.TryParse(RuleParameters, out pawnType) || !Enabled)
        {
            return;
        }

        ConfigError = true;
        ErrorMessage = $"Could not parse pawnType from {RuleParameters}, disabling rule";
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        return pawnType != MarkThatPawn.PawnType.Default && pawn.GetPawnTypes().Contains(pawnType);
    }

    private void showPawnTypeSelectorMenu()
    {
        var pawnTypeList = new List<FloatMenuOption>();

        foreach (var typeOfPawn in (MarkThatPawn.PawnType[])Enum.GetValues(typeof(MarkThatPawn.PawnType)))
        {
            switch (typeOfPawn)
            {
                case MarkThatPawn.PawnType.Default:
                case MarkThatPawn.PawnType.Slave when !ModLister.RoyaltyInstalled:
                case MarkThatPawn.PawnType.Vehicle when !MarkThatPawn.VehiclesLoaded:
                    continue;
                default:
                    pawnTypeList.Add(new FloatMenuOption($"MTP.PawnType.{typeOfPawn}".Translate(), () =>
                    {
                        RuleParameters = typeOfPawn.ToString();
                        pawnType = typeOfPawn;
                    }));
                    break;
            }
        }

        Find.WindowStack.Add(new FloatMenu(pawnTypeList));
    }
}