using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class TitleMarkerRule : MarkerRule
{
    private List<RoyalTitleDef> titleDefs = [];

    public TitleMarkerRule()
    {
        RuleType = AutoRuleType.Title;
        SetDefaultValues();
    }

    public TitleMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Title;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && titleDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var titleArea = rect.LeftPart(0.75f);
        if (edit)
        {
            var buttonLabel = "MTP.NoneSelected".Translate();
            if (titleDefs.Any())
            {
                buttonLabel = "MTP.SomeSelected".Translate(titleDefs.Count);
            }

            if (Widgets.ButtonText(titleArea.TopHalf(), buttonLabel))
            {
                showTitleSelectorMenu();
            }

            TooltipHandler.TipRegion(titleArea.TopHalf(),
                string.Join("\n", titleDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
        }
        else
        {
            var weaponLabel = "MTP.NoneSelected".Translate();
            if (titleDefs.Any())
            {
                weaponLabel = "MTP.SomeSelected".Translate(titleDefs.Count);
            }

            Widgets.Label(titleArea.TopHalf(), weaponLabel);
            TooltipHandler.TipRegion(titleArea.TopHalf(),
                string.Join("\n", titleDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
        }
    }

    public override MarkerRule GetCopy()
    {
        return new TitleMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        switch (RuleParameters)
        {
            case null:
            case "" when !Enabled:
                return;
        }

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);

        var titlePart = ruleParametersSplitted[0];
        titleDefs = [];

        foreach (var titleDefname in titlePart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var titleDef = DefDatabase<RoyalTitleDef>.GetNamedSilentFail(titleDefname);
            if (titleDef == null)
            {
                ErrorMessage = $"Could not find title with defname {titleDefname}";
                continue;
            }

            titleDefs.Add(titleDef);
        }

        if (titleDefs.Any())
        {
            return;
        }

        ErrorMessage = $"Could not find title based on {titlePart}, disabling rule";
        ConfigError = true;
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        return base.AppliesToPawn(pawn) && titleDefs.Any(def => pawn.royalty?.HasTitle(def) == true);
    }

    private void showTitleSelectorMenu()
    {
        var titleList = new List<FloatMenuOption>();

        foreach (var title in MarkThatPawn.AllTitles)
        {
            if (titleDefs.Contains(title))
            {
                titleList.Add(new FloatMenuOption(title.LabelCap, () =>
                {
                    titleDefs.Remove(title);
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), titleDefs.Select(thingDef => thingDef.defName))}";
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            titleList.Add(new FloatMenuOption(title.LabelCap, () =>
            {
                titleDefs.Add(title);
                RuleParameters =
                    $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), titleDefs.Select(thingDef => thingDef.defName))}";
            }));
        }

        Find.WindowStack.Add(new FloatMenu(titleList));
    }
}