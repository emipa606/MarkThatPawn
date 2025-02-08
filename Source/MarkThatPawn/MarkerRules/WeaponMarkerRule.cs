using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class WeaponMarkerRule : MarkerRule
{
    private bool onlyWhenDrafted;
    private bool or;
    private List<ThingDef> weaponThingDefs = [];

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
        return base.CanEnable() && weaponThingDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var weaponArea = rect.LeftPart(0.75f);
        if (edit)
        {
            var buttonLabel = "MTP.NoneSelected".Translate();
            if (weaponThingDefs.Any())
            {
                buttonLabel = "MTP.SomeSelected".Translate(weaponThingDefs.Count);
            }

            if (Widgets.ButtonText(weaponArea.TopHalf(), buttonLabel))
            {
                showWeaponSelectorMenu();
            }

            TooltipHandler.TipRegion(weaponArea.TopHalf(),
                string.Join("\n", weaponThingDefs.Select(thingDef => thingDef.LabelCap).ToArray()));

            if (weaponThingDefs.Any())
            {
                var originalValue = onlyWhenDrafted;
                Widgets.CheckboxLabeled(weaponArea.BottomHalf().LeftHalf(), "MTP.OnlyWhenDrafted".Translate(),
                    ref onlyWhenDrafted);
                TooltipHandler.TipRegion(weaponArea.BottomHalf().LeftHalf(), "MTP.OnlyWhenDraftedTT".Translate());
                if (originalValue != onlyWhenDrafted)
                {
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), weaponThingDefs.Select(thingDef => thingDef.defName).ToArray())}{MarkThatPawn.RuleItemsSplitter}{onlyWhenDrafted}{MarkThatPawn.RuleItemsSplitter}{or}";
                }

                originalValue = or;
                Widgets.CheckboxLabeled(weaponArea.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate(),
                    ref or);
                TooltipHandler.TipRegion(weaponArea.BottomHalf().RightHalf().RightPart(0.8f),
                    "MTP.OrLogicTT".Translate());
                if (originalValue != or)
                {
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), weaponThingDefs.Select(thingDef => thingDef.defName).ToArray())}{MarkThatPawn.RuleItemsSplitter}{onlyWhenDrafted}{MarkThatPawn.RuleItemsSplitter}{or}";
                }
            }
        }
        else
        {
            var weaponLabel = "MTP.NoneSelected".Translate();
            if (weaponThingDefs.Any())
            {
                weaponLabel = "MTP.SomeSelected".Translate(weaponThingDefs.Count);
            }

            Widgets.Label(weaponArea.TopHalf(), weaponLabel);
            TooltipHandler.TipRegion(weaponArea.TopHalf(),
                string.Join("\n", weaponThingDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
            if (onlyWhenDrafted)
            {
                Widgets.Label(weaponArea.BottomHalf().LeftHalf(), "MTP.OnlyWhenDrafted".Translate());
                TooltipHandler.TipRegion(weaponArea.BottomHalf().LeftHalf(), "MTP.OnlyWhenDraftedTT".Translate());
            }

            if (or)
            {
                Widgets.Label(weaponArea.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate());
                TooltipHandler.TipRegion(weaponArea.BottomHalf().RightHalf().RightPart(0.8f),
                    "MTP.OrLogicTT".Translate());
            }
        }

        if (!weaponThingDefs.Any())
        {
            return;
        }

        var weaponImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(weaponImageRect,
            string.Join("\n", weaponThingDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
        GUI.DrawTexture(weaponImageRect, Widgets.GetIconFor(weaponThingDefs.First()));
        if (weaponThingDefs.Count > 1)
        {
            GUI.DrawTexture(weaponImageRect, MarkThatPawn.MultiIconOverlay.mainTexture);
        }
    }

    public override MarkerRule GetCopy()
    {
        return new WeaponMarkerRule(GetBlob());
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

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);
        var weaponPart = ruleParametersSplitted[0];
        weaponThingDefs = [];

        foreach (var weaponDefname in weaponPart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var weaponDef = DefDatabase<ThingDef>.GetNamedSilentFail(weaponDefname);
            if (weaponDef == null)
            {
                ErrorMessage = $"Could not find weapon with defname {weaponDefname}";
                continue;
            }

            weaponThingDefs.Add(weaponDef);
        }

        if (!weaponThingDefs.Any())
        {
            ErrorMessage = $"Could not find weapons based on {weaponPart}, disabling rule";
            ConfigError = true;
            return;
        }

        if (ruleParametersSplitted.Length == 1)
        {
            return;
        }

        if (!bool.TryParse(ruleParametersSplitted[1], out onlyWhenDrafted))
        {
            ErrorMessage = $"Could not parse bool for {ruleParametersSplitted[1]}, disabling rule";
            ConfigError = true;
        }

        if (ruleParametersSplitted.Length != 3)
        {
            return;
        }

        if (bool.TryParse(ruleParametersSplitted[2], out or))
        {
            return;
        }

        ErrorMessage = $"Could not parse bool for {ruleParametersSplitted[2]}, disabling rule";
        ConfigError = true;
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
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

        if (or)
        {
            return pawn.equipment.AllEquipmentListForReading.Any(weapon => weaponThingDefs.Contains(weapon.def));
        }

        var allEquippedDefs = pawn.equipment.AllEquipmentListForReading.Select(weapon => weapon.def);
        return weaponThingDefs.All(def => allEquippedDefs.Contains(def));
    }


    private void showWeaponSelectorMenu()
    {
        var weaponList = new List<FloatMenuOption>();

        foreach (var weapon in MarkThatPawn.AllValidWeapons)
        {
            if (weaponThingDefs.Contains(weapon))
            {
                weaponList.Add(new FloatMenuOption(weapon.LabelCap, () =>
                {
                    weaponThingDefs.Remove(weapon);
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), weaponThingDefs.Select(thingDef => thingDef.defName))}{MarkThatPawn.RuleItemsSplitter}{onlyWhenDrafted}{MarkThatPawn.RuleItemsSplitter}{or}";
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            weaponList.Add(new FloatMenuOption(weapon.LabelCap, () =>
            {
                weaponThingDefs.Add(weapon);
                RuleParameters =
                    $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), weaponThingDefs.Select(thingDef => thingDef.defName))}{MarkThatPawn.RuleItemsSplitter}{onlyWhenDrafted}{MarkThatPawn.RuleItemsSplitter}{or}";
            }));
        }

        Find.WindowStack.Add(new FloatMenu(weaponList));
    }
}