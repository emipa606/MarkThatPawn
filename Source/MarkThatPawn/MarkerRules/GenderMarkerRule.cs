using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class GenderMarkerRule : MarkerRule
{
    private bool animal;
    private bool male;

    public GenderMarkerRule()
    {
        RuleType = AutoRuleType.Gender;
        male = true;
        animal = false;
        RuleParameters = $"{male}|{animal}";
        SetDefaultValues();
    }

    public GenderMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Gender;
        male = true;
        animal = false;
        RuleParameters = $"{male}|{animal}";
        SetBlob(blob);
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var genderArea = rect.TopPart(0.55f);
        var animalArea = rect.BottomPart(0.45f);
        if (edit)
        {
            if (Widgets.RadioButtonLabeled(genderArea.LeftPart(0.45f), "MTP.Male".Translate(), male))
            {
                male = true;
                RuleParameters = $"{male}|{animal}";
            }

            if (Widgets.RadioButtonLabeled(genderArea.RightPart(0.45f), "MTP.Female".Translate(), !male))
            {
                male = false;
                RuleParameters = $"{male}|{animal}";
            }

            var animalWas = animal;
            Widgets.CheckboxLabeled(animalArea.LeftPart(0.45f), "MTP.Animal".Translate(), ref animal);
            if (animalWas != animal)
            {
                RuleParameters = $"{male}|{animal}";
            }

            return;
        }

        Widgets.Label(genderArea, male ? "MTP.Male".Translate() : "MTP.Female".Translate());
        Widgets.Label(animalArea, animal ? "MTP.OnlyAnimal".Translate() : "MTP.NotAnimal".Translate());
    }

    public override MarkerRule GetCopy()
    {
        return new GenderMarkerRule(GetBlob());
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

        var genderPart = RuleParameters;
        var animalPart = $"{false}";
        if (RuleParameters.Contains("|"))
        {
            genderPart = RuleParameters.Split('|')[0];
            animalPart = RuleParameters.Split('|')[1];
        }

        if (bool.TryParse(genderPart, out male) && bool.TryParse(animalPart, out animal))
        {
            return;
        }

        ErrorMessage = $"Could not parse gender-type {RuleParameters}, disabling rule";
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

        if (animal != pawn.RaceProps.Animal)
        {
            return false;
        }

        if (male)
        {
            return pawn.gender == Gender.Male;
        }

        return pawn.gender == Gender.Female;
    }
}