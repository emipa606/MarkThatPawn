using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

public class WeaponMarkerRule : MarkerRule
{
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
        var weaponArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(weaponArea, weaponThingDef?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showWeaponSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(weaponArea, weaponThingDef?.LabelCap ?? "MTP.NoneSelected".Translate());
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

        weaponThingDef = DefDatabase<ThingDef>.GetNamedSilentFail(RuleParameters);
        if (weaponThingDef != null)
        {
            return;
        }

        ErrorMessage = $"Could not find weapon with defname {RuleParameters}, disabling rule";
        ConfigError = true;
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (pawn == null || pawn.Destroyed || !pawn.Spawned)
        {
            return false;
        }

        if (pawn.equipment == null || !pawn.equipment.HasAnything())
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
                    RuleParameters = weapon.defName;
                    weaponThingDef = weapon;
                },
                Widgets.GetIconFor(weapon), Color.white));
        }

        Find.WindowStack.Add(new FloatMenu(weaponList));
    }
}