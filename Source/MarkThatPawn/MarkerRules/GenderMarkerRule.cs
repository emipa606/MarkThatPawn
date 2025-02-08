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
        RuleParameters = $"{male}{MarkThatPawn.RuleItemsSplitter}{animal}";
        SetDefaultValues();
    }

    public GenderMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Gender;
        male = true;
        animal = false;
        RuleParameters = $"{male}{MarkThatPawn.RuleItemsSplitter}{animal}";
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
                RuleParameters = $"{male}{MarkThatPawn.RuleItemsSplitter}{animal}";
            }

            if (Widgets.RadioButtonLabeled(genderArea.RightPart(0.45f), "MTP.Female".Translate(), !male))
            {
                male = false;
                RuleParameters = $"{male}{MarkThatPawn.RuleItemsSplitter}{animal}";
            }

            var animalWas = animal;
            Widgets.CheckboxLabeled(animalArea.LeftPart(0.45f), "MTP.Animal".Translate(), ref animal);
            if (animalWas != animal)
            {
                RuleParameters = $"{male}{MarkThatPawn.RuleItemsSplitter}{animal}";
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

        try
        {
            var genderPart = RuleParameters;
            var animalPart = $"{false}";
            if (RuleParameters.Contains(MarkThatPawn.RuleItemsSplitter.ToString()))
            {
                genderPart = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter)[0];
                animalPart = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter)[1];
            }

            if (!bool.TryParse(genderPart, out male))
            {
                male = true;
            }

            if (!bool.TryParse(animalPart, out animal))
            {
                animal = false;
            }
        }
        catch
        {
            ErrorMessage = $"Could not parse gender-type {RuleParameters}, disabling rule";
            ConfigError = true;
        }
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
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