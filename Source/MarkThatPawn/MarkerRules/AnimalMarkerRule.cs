using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class AnimalMarkerRule : MarkerRule
{
    private ThingDef animal;

    public AnimalMarkerRule()
    {
        RuleType = AutoRuleType.Animal;
        SetDefaultValues();
        ApplicablePawnTypes =
        [
            MarkThatPawn.PawnType.Colonist,
            MarkThatPawn.PawnType.Enemy,
            MarkThatPawn.PawnType.Neutral
        ];
    }

    public AnimalMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Animal;
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
        var AnimalArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(AnimalArea,
                    animal?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showAnimalSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(AnimalArea,
                animal?.LabelCap ?? "MTP.NoneSelected".Translate());
        }

        if (animal == null)
        {
            return;
        }

        var animalImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(animalImageRect, animal.description);
        GUI.DrawTexture(animalImageRect, Widgets.GetIconFor(animal));
    }

    public override MarkerRule GetCopy()
    {
        return new AnimalMarkerRule(GetBlob());
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

        animal = DefDatabase<ThingDef>.GetNamedSilentFail(RuleParameters);
        if (animal != null)
        {
            return;
        }

        ErrorMessage = $"Could not find animal with defname {RuleParameters}, disabling rule";
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

        return pawn.def == animal;
    }

    private void showAnimalSelectorMenu()
    {
        var AnimalList = new List<FloatMenuOption>();

        foreach (var animalToSelect in MarkThatPawn.AllAnimals)
        {
            AnimalList.Add(new FloatMenuOption(animalToSelect.LabelCap, () =>
            {
                RuleParameters = animalToSelect.defName;
                animal = animalToSelect;
            }));
        }

        Find.WindowStack.Add(new FloatMenu(AnimalList));
    }
}