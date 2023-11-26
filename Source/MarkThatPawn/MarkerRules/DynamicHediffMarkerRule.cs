using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class DynamicHediffMarkerRule : MarkerRule
{
    private Dictionary<HediffDef, int> HediffDefs;

    public DynamicHediffMarkerRule()
    {
        RuleType = AutoRuleType.HediffDynamic;
        HediffDefs = new Dictionary<HediffDef, int>();
        SetDefaultValues();
        IsOverride = true;
    }

    public DynamicHediffMarkerRule(string blob)
    {
        RuleType = AutoRuleType.HediffDynamic;
        HediffDefs = new Dictionary<HediffDef, int>();
        SetBlob(blob);
        IsOverride = true;
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && HediffDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var hediffListRect = rect;

        if (edit)
        {
            hediffListRect = rect.RightPart(0.75f);
            var buttonRect = rect.LeftPart(0.23f);
            if (Widgets.ButtonText(buttonRect,
                    !HediffDefs.Any()
                        ? "MTP.NoneSelected".Translate()
                        : "MTP.SomeSelected".Translate(HediffDefs.Count)))
            {
                showHediffSelectorMenu();
            }
        }

        if (!HediffDefs.Any())
        {
            Widgets.Label(hediffListRect, "MTP.NoneSelected".Translate());
            return;
        }

        var labelList = new List<string>();
        foreach (var keyValuePair in HediffDefs)
        {
            var label = MarkThatPawn.GetDistinctHediffName(keyValuePair.Key, MarkThatPawn.AllDynamicHediffs);

            if (keyValuePair.Key.stages is { Count: > 1 })
            {
                label +=
                    $":  {keyValuePair.Key.stages[keyValuePair.Value].label ?? "MTP.AutomaticType.HediffStage".Translate(keyValuePair.Value)}";
            }

            labelList.Add(label);
        }

        Widgets.Label(hediffListRect, string.Join(", ", labelList));
    }

    public override MarkerRule GetCopy()
    {
        return new DynamicHediffMarkerRule(GetBlob());
    }

    protected override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        HediffDefs = new Dictionary<HediffDef, int>();

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        foreach (var hediffKeyPair in RuleParameters.Split(','))
        {
            if (!hediffKeyPair.Contains("|") || hediffKeyPair.Split('|').Length != 2)
            {
                ConfigError = true;
                continue;
            }

            var hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffKeyPair.Split('|')[0]);
            if (hediffDef == null)
            {
                ConfigError = true;
                continue;
            }

            if (!int.TryParse(hediffKeyPair.Split('|')[1], out var hediffStage))
            {
                ConfigError = true;
                continue;
            }

            if (hediffDef.stages != null && hediffDef.stages.Count <= hediffStage)
            {
                ConfigError = true;
                continue;
            }

            HediffDefs[hediffDef] = hediffStage;
        }

        if (ConfigError)
        {
            ErrorMessage = $"Could not parse all HediffDefs from {RuleParameters}, disabling rule";
        }
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

        foreach (var hediffDef in HediffDefs)
        {
            if (!pawnHediffs.Any(hediff => hediff.def == hediffDef.Key && hediff.CurStageIndex >= hediffDef.Value))
            {
                return false;
            }
        }

        return true;
    }

    private void showHediffSelectorMenu()
    {
        var hediffMenu = new List<FloatMenuOption>();

        foreach (var hediffDef in MarkThatPawn.AllDynamicHediffs)
        {
            var label = MarkThatPawn.GetDistinctHediffName(hediffDef, MarkThatPawn.AllDynamicHediffs);

            if (HediffDefs.Any(pair => pair.Key == hediffDef))
            {
                hediffMenu.Add(new FloatMenuOption(label, () =>
                {
                    HediffDefs.Remove(hediffDef);
                    RuleParameters = string.Join(",",
                        HediffDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}"));
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            if (hediffDef.stages == null || hediffDef.stages.Count < 2)
            {
                hediffMenu.Add(new FloatMenuOption(label, () =>
                {
                    HediffDefs[hediffDef] = 0;
                    RuleParameters = string.Join(",",
                        HediffDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}"));
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
                            HediffDefs[hediffDef] = localIndex;
                            RuleParameters = string.Join(",",
                                HediffDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}"));
                        }));
                }

                Find.WindowStack.Add(new FloatMenu(subMenu));
            }, MarkThatPawn.ExpandIcon, Color.white, iconJustification: HorizontalJustification.Right));
        }

        Find.WindowStack.Add(new FloatMenu(hediffMenu));
    }
}