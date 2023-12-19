using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class WeaponMarkerRule : MarkerRule
{
    private bool onlyWhenDrafted;
    private ThingDef weaponThingDef;

    public WeaponMarkerRule()
    {
        RuleType = AutoRuleType.Weapon;
        SetDefaultValues();
    }

    public WeaponMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Weapon;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && weaponThingDef != null;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var weaponArea = rect.LeftPart(0.75f);
        if (edit)
        {
            if (Widgets.ButtonText(weaponArea.TopHalf(), weaponThingDef?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showWeaponSelectorMenu();
            }

            if (weaponThingDef != null)
            {
                var originalValue = onlyWhenDrafted;
                Widgets.CheckboxLabeled(weaponArea.BottomHalf(), "MTP.OnlyWhenDrafted".Translate(),
                    ref onlyWhenDrafted);
                if (originalValue != onlyWhenDrafted)
                {
                    RuleParameters = $"{weaponThingDef.defName}|{onlyWhenDrafted}";
                }
            }
        }
        else
        {
            Widgets.Label(weaponArea.TopHalf(), weaponThingDef?.LabelCap ?? "MTP.NoneSelected".Translate());
            if (onlyWhenDrafted)
            {
                Widgets.Label(weaponArea.BottomHalf(), "MTP.OnlyWhenDrafted".Translate());
            }
        }

        if (weaponThingDef == null)
        {
            return;
        }

        var weaponImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(weaponImageRect, weaponThingDef.description);
        GUI.DrawTexture(weaponImageRect, Widgets.GetIconFor(weaponThingDef));
    }

    public override MarkerRule GetCopy()
    {
        return new WeaponMarkerRule(GetBlob());
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

        var weaponPart = RuleParameters.Split('|')[0];

        weaponThingDef = DefDatabase<ThingDef>.GetNamedSilentFail(weaponPart);
        if (weaponThingDef == null)
        {
            ErrorMessage = $"Could not find weapon with defname {weaponPart}, disabling rule";
            ConfigError = true;
            return;
        }

        if (!RuleParameters.Contains("|"))
        {
            return;
        }

        if (bool.TryParse(RuleParameters.Split('|')[1], out onlyWhenDrafted))
        {
            return;
        }

        ErrorMessage = $"Could not parse bool in {RuleParameters}, disabling rule";
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
            return false;
        }

        if (onlyWhenDrafted && !pawn.Drafted)
        {
            return false;
        }

        return pawn.equipment.AllEquipmentListForReading.Any(thing => thing.def == weaponThingDef);
    }


    private void showWeaponSelectorMenu()
    {
        var weaponList = new List<FloatMenuOption>();

        foreach (var weapon in MarkThatPawn.AllValidWeapons)
        {
            weaponList.Add(new FloatMenuOption(weapon.LabelCap, () =>
            {
                RuleParameters = $"{weapon.defName}|{onlyWhenDrafted}";
                weaponThingDef = weapon;
            }, weapon));
        }

        Find.WindowStack.Add(new FloatMenu(weaponList));
    }
}