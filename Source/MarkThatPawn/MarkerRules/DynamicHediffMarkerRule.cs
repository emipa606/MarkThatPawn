using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class DynamicHediffMarkerRule : MarkerRule
{
    private Dictionary<HediffDef, int> hediffDefs;
    private bool or;

    public DynamicHediffMarkerRule()
    {
        RuleType = AutoRuleType.HediffDynamic;
        hediffDefs = new Dictionary<HediffDef, int>();
        SetDefaultValues();
        IsOverride = true;
    }

    public DynamicHediffMarkerRule(string blob)
    {
        RuleType = AutoRuleType.HediffDynamic;
        hediffDefs = new Dictionary<HediffDef, int>();
        SetBlob(blob);
        IsOverride = true;
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && hediffDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var hediffRect = rect.TopHalf();

        var hediffList = new List<string>();
        foreach (var keyValuePair in hediffDefs)
        {
            var label = MarkThatPawn.GetDistinctHediffName(keyValuePair.Key, MarkThatPawn.AllDynamicHediffs);

            if (keyValuePair.Key.stages is { Count: > 1 })
            {
                label +=
                    $":  {keyValuePair.Key.stages[keyValuePair.Value].label ?? "MTP.AutomaticType.HediffStage".Translate(keyValuePair.Value)}";
            }

            hediffList.Add(label);
        }

        var hediffListLabel = string.Join("\n", hediffList);

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
                    $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), hediffDefs.Select(pair => $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
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
        return new DynamicHediffMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        hediffDefs = new Dictionary<HediffDef, int>();

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        if (!RuleParameters.Contains(MarkThatPawn.RuleAlternateItemsSplitter) && RuleParameters.Contains(','))
        {
            RuleParameters = RuleParameters.Replace(',', MarkThatPawn.RuleAlternateItemsSplitter);
            RuleParameters = RuleParameters.Replace(MarkThatPawn.RuleItemsSplitter, MarkThatPawn.RuleInternalSplitter);
        }

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);
        var hediffPart = ruleParametersSplitted[0];

        foreach (var hediffKeyPair in hediffPart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var traitSplitted = hediffKeyPair.Split(MarkThatPawn.RuleInternalSplitter);

            if (traitSplitted.Length != 2)
            {
                ConfigError = true;
                continue;
            }

            var hediffDef =
                DefDatabase<HediffDef>.GetNamedSilentFail(traitSplitted[0]);
            if (hediffDef == null)
            {
                ConfigError = true;
                continue;
            }

            if (!int.TryParse(traitSplitted[1], out var hediffStage))
            {
                ConfigError = true;
                continue;
            }

            if (hediffDef.stages != null && hediffDef.stages.Count <= hediffStage)
            {
                ConfigError = true;
                continue;
            }

            hediffDefs[hediffDef] = hediffStage;
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

        var pawnHediffs = pawn.health?.hediffSet?.hediffs;

        if (pawnHediffs == null)
        {
            return false;
        }

        return or
            ? pawnHediffs.Any(hediff =>
                hediffDefs.Any(pair => pair.Key == hediff.def && pair.Value <= hediff.CurStageIndex))
            : hediffDefs.All(hediffDef =>
                pawnHediffs.Any(hediff => hediff.def == hediffDef.Key && hediff.CurStageIndex >= hediffDef.Value));
    }

    private void showHediffSelectorMenu()
    {
        var hediffMenu = new List<FloatMenuOption>();

        foreach (var hediffDef in MarkThatPawn.AllDynamicHediffs)
        {
            var label = MarkThatPawn.GetDistinctHediffName(hediffDef, MarkThatPawn.AllDynamicHediffs);

            if (hediffDefs.Any(pair => pair.Key == hediffDef))
            {
                hediffMenu.Add(new FloatMenuOption(label, () =>
                {
                    hediffDefs.Remove(hediffDef);
                    RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                        hediffDefs.Select(pair => $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            if (hediffDef.stages == null || hediffDef.stages.Count < 2)
            {
                hediffMenu.Add(new FloatMenuOption(label, () =>
                {
                    hediffDefs[hediffDef] = 0;
                    RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                        hediffDefs.Select(pair => $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                }));
                continue;
            }

            hediffMenu.Add(new FloatMenuOption(label, () =>
            {
                var subMenu = new List<FloatMenuOption>();
                for (var index = 0; index < hediffDef.stages.Count; index++)
                {
                    var localIndex = index;
                    var subLabel = $"{label} {"MTP.AutomaticType.HediffStage".Translate(index)}";

                    if (!string.IsNullOrEmpty(hediffDef.stages[index].label))
                    {
                        subLabel += $": {hediffDef.stages[index].label}";
                    }

                    subMenu.Add(new FloatMenuOption(subLabel,
                        () =>
                        {
                            hediffDefs[hediffDef] = localIndex;
                            RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                                hediffDefs.Select(pair =>
                                    $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                        }));
                }

                Find.WindowStack.Add(new FloatMenu(subMenu));
            }, MarkThatPawn.ExpandIcon, Color.white, iconJustification: HorizontalJustification.Right));
        }

        Find.WindowStack.Add(new FloatMenu(hediffMenu));
    }
}