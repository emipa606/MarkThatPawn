using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class StaticHediffMarkerRule : MarkerRule
{
    private List<HediffDef> hediffDefs;
    private bool or;

    public StaticHediffMarkerRule()
    {
        RuleType = AutoRuleType.HediffStatic;
        hediffDefs = [];
        SetDefaultValues();
    }

    public StaticHediffMarkerRule(string blob)
    {
        RuleType = AutoRuleType.HediffStatic;
        hediffDefs = [];
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && hediffDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var hediffRect = rect.TopHalf();

        var hediffList = new List<string>();
        foreach (var hediffDef in hediffDefs)
        {
            hediffList.Add(MarkThatPawn.GetDistinctHediffName(hediffDef, MarkThatPawn.AllStaticHediffs));
        }

        var hediffListLabel = string.Join(", ", hediffList);

        if (edit)
        {
            if (Widgets.ButtonText(hediffRect,
                    !hediffDefs.Any()
                        ? "MTP.NoneSelected".Translate()
                        : "MTP.SomeSelected".Translate(hediffDefs.Count)))
            {
                showHediffSelectorMenu();
            }

            TooltipHandler.TipRegion(hediffRect, hediffListLabel);
            var originalValue = or;
            Widgets.CheckboxLabeled(rect.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate(), ref or);
            TooltipHandler.TipRegion(rect.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogicTT".Translate());
            if (originalValue != or)
            {
                RuleParameters =
                    $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), hediffDefs.Select(def => def.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
            }

            return;
        }

        if (!hediffDefs.Any())
        {
            Widgets.Label(hediffRect, "MTP.NoneSelected".Translate());
            return;
        }

        Widgets.Label(hediffRect, "MTP.SomeSelected".Translate(hediffDefs.Count));
        TooltipHandler.TipRegion(hediffRect, hediffListLabel);

        if (!or)
        {
            return;
        }

        Widgets.Label(rect.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate());
        TooltipHandler.TipRegion(rect.BottomHalf().RightHalf().RightPart(0.8f),
            "MTP.OrLogicTT".Translate());
    }

    public override MarkerRule GetCopy()
    {
        return new StaticHediffMarkerRule(GetBlob());
    }

    protected override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        hediffDefs = [];

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        if (!RuleParameters.Contains(MarkThatPawn.RuleAlternateItemsSplitter) && RuleParameters.Contains(','))
        {
            RuleParameters = RuleParameters.Replace(',', MarkThatPawn.RuleAlternateItemsSplitter);
        }

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);
        var hediffPart = ruleParametersSplitted[0];

        foreach (var defName in hediffPart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(defName);
            if (hediffDef != null)
            {
                hediffDefs.Add(hediffDef);
                continue;
            }

            ConfigError = true;
        }

        if (ConfigError)
        {
            ErrorMessage = $"Could not parse all hediffDefs from {RuleParameters}, disabling rule";
        }

        if (ruleParametersSplitted.Length == 1)
        {
            return;
        }

        if (bool.TryParse(ruleParametersSplitted[1], out or))
        {
            return;
        }

        ErrorMessage = $"Could not parse bool for {ruleParametersSplitted[1]}, disabling rule";
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

        var pawnHediffs = pawn.health?.hediffSet?.hediffs;

        if (pawnHediffs == null)
        {
            return false;
        }

        return or
            ? pawnHediffs.Any(hediff => hediffDefs.Contains(hediff.def))
            : hediffDefs.All(hediffDef => pawnHediffs.Any(hediff => hediff.def == hediffDef));
    }

    private void showHediffSelectorMenu()
    {
        var hediffMenu = new List<FloatMenuOption>();

        foreach (var hediffDef in MarkThatPawn.AllStaticHediffs)
        {
            var label = MarkThatPawn.GetDistinctHediffName(hediffDef, MarkThatPawn.AllStaticHediffs);

            if (hediffDefs.Any(def => def == hediffDef))
            {
                hediffMenu.Add(new FloatMenuOption(label, () =>
                {
                    hediffDefs.Remove(hediffDef);
                    RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                        hediffDefs.Select(def => def.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            hediffMenu.Add(new FloatMenuOption(label, () =>
            {
                hediffDefs.Add(hediffDef);
                RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                    hediffDefs.Select(def => def.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
            }));
        }

        Find.WindowStack.Add(new FloatMenu(hediffMenu));
    }
}