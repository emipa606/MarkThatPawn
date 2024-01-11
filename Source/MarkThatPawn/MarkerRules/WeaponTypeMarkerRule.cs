using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class WeaponTypeMarkerRule : MarkerRule
{
    public enum EquippedWeaponType
    {
        None,
        Melee,
        Ranged,
        RangedExplosive,
        Thrown
    }

    private EquippedWeaponType equippedWeaponType;

    public WeaponTypeMarkerRule()
    {
        RuleType = AutoRuleType.WeaponType;
        equippedWeaponType = EquippedWeaponType.None;
        SetDefaultValues();
    }

    public WeaponTypeMarkerRule(string blob)
    {
        RuleType = AutoRuleType.WeaponType;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && equippedWeaponType != EquippedWeaponType.None;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var weaponArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(weaponArea,
                    equippedWeaponType == EquippedWeaponType.None
                        ? "MTP.NoneSelected".Translate()
                        : $"MTP.EquippedWeaponType.{equippedWeaponType}".Translate()))
            {
                showWeaponTypeSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(weaponArea,
                equippedWeaponType == EquippedWeaponType.None
                    ? "MTP.NoneSelected".Translate()
                    : $"MTP.EquippedWeaponType.{equippedWeaponType}".Translate());
        }
    }

    public override MarkerRule GetCopy()
    {
        return new WeaponTypeMarkerRule(GetBlob());
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

        if (Enum.TryParse(RuleParameters, out equippedWeaponType))
        {
            return;
        }

        ErrorMessage = $"Could not parse weapon-type {RuleParameters}, disabling rule";
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

        if (pawn.equipment == null || !pawn.equipment.HasAnything())
        {
            return equippedWeaponType == EquippedWeaponType.Melee;
        }

        var allEquipment = pawn.equipment.AllEquipmentListForReading;
        switch (equippedWeaponType)
        {
            case EquippedWeaponType.Melee:
                return allEquipment.Any(thing => thing.def.IsMeleeWeapon);
            case EquippedWeaponType.Ranged:
                return allEquipment.Any(thing => thing.def.IsRangedWeapon);
            case EquippedWeaponType.RangedExplosive:
                return allEquipment.Any(thing => MarkThatPawn.AllExplosiveRangedWeapons.Contains(thing.def));
            case EquippedWeaponType.Thrown:
                return allEquipment.Any(thing => MarkThatPawn.AllThrownWeapons.Contains(thing.def));
        }

        return false;
    }

    private void showWeaponTypeSelectorMenu()
    {
        var weaponList = new List<FloatMenuOption>();

        foreach (var weaponType in (EquippedWeaponType[])Enum.GetValues(typeof(EquippedWeaponType)))
        {
            if (weaponType == EquippedWeaponType.None)
            {
                continue;
            }

            weaponList.Add(new FloatMenuOption($"MTP.EquippedWeaponType.{weaponType}".Translate(), () =>
            {
                RuleParameters = weaponType.ToString();
                equippedWeaponType = weaponType;
            }));
        }

        Find.WindowStack.Add(new FloatMenu(weaponList));
    }
}