using System;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class AgeMarkerRule : MarkerRule
{
    private string highBuffer;
    private int highLimit;
    private string lowBuffer;
    private int lowLimit;

    public AgeMarkerRule()
    {
        RuleType = AutoRuleType.Age;
        lowLimit = 0;
        highLimit = 0;
        RuleParameters = $"{lowLimit}{MarkThatPawn.RuleItemsSplitter}{highLimit}";
        SetDefaultValues();
    }

    public AgeMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Age;
        lowLimit = 0;
        highLimit = 0;
        RuleParameters = $"{lowLimit}{MarkThatPawn.RuleItemsSplitter}{highLimit}";
        SetBlob(blob);
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var lowArea = rect.TopHalf();
        var highArea = rect.BottomHalf();
        if (edit)
        {
            var highUnlimited = highLimit == -1;
            var lowWas = lowLimit;
            Widgets.TextFieldNumericLabeled(lowArea.LeftPart(0.8f), "MTP.LowAgeLimit".Translate(""), ref lowLimit,
                ref lowBuffer);
            if (lowWas != lowLimit && !highUnlimited)
            {
                highLimit = Math.Max(highLimit, lowLimit);
                highBuffer = highLimit.ToString();
            }

            if (highUnlimited)
            {
                Widgets.Label(highArea.LeftPart(0.8f), "MTP.Unlimited".Translate());
            }
            else
            {
                var highWas = highLimit;
                Widgets.TextFieldNumericLabeled(highArea.LeftPart(0.8f), "MTP.HighAgeLimit".Translate(""),
                    ref highLimit, ref highBuffer);
                if (highWas != highLimit)
                {
                    lowLimit = Math.Min(lowLimit, highLimit);
                    lowBuffer = lowLimit.ToString();
                }
            }

            Widgets.Checkbox(highArea.RightPart(0.1f).position, ref highUnlimited);
            Widgets.ButtonImageFitted(highArea.RightPart(0.2f).LeftHalf(), TexButton.Infinity);
            if (highUnlimited)
            {
                highLimit = -1;
            }
            else
            {
                highLimit = Math.Max(highLimit, lowLimit);
                highBuffer = highLimit.ToString();
            }

            RuleParameters = $"{lowLimit}{MarkThatPawn.RuleItemsSplitter}{highLimit}";
            return;
        }

        Widgets.Label(lowArea, lowLimit > -1 ? "MTP.LowAgeLimit".Translate(lowLimit) : "MTP.LowAgeLimit".Translate(0));
        Widgets.Label(highArea, highLimit > -1 ? "MTP.HighAgeLimit".Translate(highLimit) : "MTP.Unlimited".Translate());
    }

    protected override bool CanEnable()
    {
        if (!base.CanEnable())
        {
            return false;
        }

        return highLimit == -1 || highLimit >= lowLimit;
    }

    public override MarkerRule GetCopy()
    {
        return new AgeMarkerRule(GetBlob());
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

        if (!RuleParameters.Contains(MarkThatPawn.RuleItemsSplitter.ToString()))
        {
            return;
        }

        var lowPart = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter)[0];
        var highPart = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter)[1];
        if (int.TryParse(lowPart, out lowLimit) && int.TryParse(highPart, out highLimit))
        {
            return;
        }

        ErrorMessage = $"Could not parse Age-type {RuleParameters}, disabling rule";
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

        if (pawn.ageTracker.AgeBiologicalYears < lowLimit)
        {
            return false;
        }

        return highLimit == -1 || pawn.ageTracker.AgeBiologicalYears <= highLimit;
    }
}