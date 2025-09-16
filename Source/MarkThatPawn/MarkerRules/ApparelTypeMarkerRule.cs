using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class ApparelTypeMarkerRule : MarkerRule
{
    private EquippedApparelType equippedApparelType;

    public ApparelTypeMarkerRule()
    {
        RuleType = AutoRuleType.ApparelType;
        equippedApparelType = EquippedApparelType.None;
        SetDefaultValues();
    }

    public ApparelTypeMarkerRule(string blob)
    {
        RuleType = AutoRuleType.ApparelType;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && equippedApparelType != EquippedApparelType.None;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var apparelArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(apparelArea,
                    equippedApparelType == EquippedApparelType.None
                        ? "MTP.NoneSelected".Translate()
                        : $"MTP.EquippedApparelType.{equippedApparelType}".Translate()))
            {
                showApparelTypeSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(apparelArea,
                equippedApparelType == EquippedApparelType.None
                    ? "MTP.NoneSelected".Translate()
                    : $"MTP.EquippedApparelType.{equippedApparelType}".Translate());
        }
    }

    public override MarkerRule GetCopy()
    {
        return new ApparelTypeMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        switch (RuleParameters)
        {
            case null:
            case "" when !Enabled:
                return;
        }

        if (Enum.TryParse(RuleParameters, out equippedApparelType))
        {
            return;
        }

        ErrorMessage = $"Could not parse Apparel-type {RuleParameters}, disabling rule";
        ConfigError = true;
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        if (pawn.apparel is not { AnyApparel: true })
        {
            return equippedApparelType == EquippedApparelType.None;
        }

        var allApparel = pawn.apparel.WornApparel;
        switch (equippedApparelType)
        {
            case EquippedApparelType.Royal:
                return allApparel.Any(thing => MarkThatPawn.AllRoyalApparel.Contains(thing.def));
            case EquippedApparelType.Psycast:
                return allApparel.Any(thing => MarkThatPawn.AllPsycastApparel.Contains(thing.def));
            case EquippedApparelType.Mechanator:
                return allApparel.Any(thing => MarkThatPawn.AllMechanatorApparel.Contains(thing.def));
            case EquippedApparelType.Armored:
                return allApparel.Any(thing => MarkThatPawn.AllArmoredApparel.Contains(thing.def));
            case EquippedApparelType.EnviromentalProtection:
                return allApparel.Any(thing => MarkThatPawn.AllEnviromentalProtectionApparel.Contains(thing.def));
            case EquippedApparelType.Basic:
                return allApparel.Any(thing => MarkThatPawn.AllBasicApparel.Contains(thing.def));
        }

        return false;
    }

    private void showApparelTypeSelectorMenu()
    {
        var apparelList = new List<FloatMenuOption>();

        foreach (var apparelType in (EquippedApparelType[])Enum.GetValues(typeof(EquippedApparelType)))
        {
            if (apparelType == EquippedApparelType.None)
            {
                continue;
            }

            if (!ModLister.BiotechInstalled && apparelType == EquippedApparelType.Mechanator)
            {
                continue;
            }

            if (!ModLister.RoyaltyInstalled && apparelType is EquippedApparelType.Psycast or EquippedApparelType.Royal)
            {
                continue;
            }

            apparelList.Add(new FloatMenuOption($"MTP.EquippedApparelType.{apparelType}".Translate(), () =>
            {
                RuleParameters = apparelType.ToString();
                equippedApparelType = apparelType;
            }));
        }

        Find.WindowStack.Add(new FloatMenu(apparelList));
    }

    private enum EquippedApparelType
    {
        None,
        Royal,
        Psycast,
        Mechanator,
        Armored,
        EnviromentalProtection,
        Basic
    }
}