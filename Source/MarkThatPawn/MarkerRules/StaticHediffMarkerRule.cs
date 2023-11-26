using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class StaticHediffMarkerRule : MarkerRule
{
    private List<HediffDef> HediffDefs;

    public StaticHediffMarkerRule()
    {
        RuleType = AutoRuleType.HediffStatic;
        HediffDefs = [];
        SetDefaultValues();
    }

    public StaticHediffMarkerRule(string blob)
    {
        RuleType = AutoRuleType.HediffStatic;
        HediffDefs = [];
        SetBlob(blob);
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
        foreach (var hediffDef in HediffDefs)
        {
            labelList.Add(MarkThatPawn.GetDistinctHediffName(hediffDef, MarkThatPawn.AllStaticHediffs));
        }

        Widgets.Label(hediffListRect, string.Join(", ", labelList));
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

        HediffDefs = [];

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        foreach (var defName in RuleParameters.Split(','))
        {
            var hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(defName);
            if (hediffDef != null)
            {
                HediffDefs.Add(hediffDef);
                continue;
            }

            ConfigError = true;
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
            if (!pawnHediffs.Any(hediff => hediff.def == hediffDef))
            {
                return false;
            }
        }

        return true;
    }

    private void showHediffSelectorMenu()
    {
        var hediffMenu = new List<FloatMenuOption>();

        foreach (var hediffDef in MarkThatPawn.AllStaticHediffs)
        {
            var label = MarkThatPawn.GetDistinctHediffName(hediffDef, MarkThatPawn.AllStaticHediffs);

            if (HediffDefs.Any(def => def == hediffDef))
            {
                hediffMenu.Add(new FloatMenuOption(label, () =>
                {
                    HediffDefs.Remove(hediffDef);
                    RuleParameters = string.Join(",", HediffDefs.Select(def => def.defName));
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            hediffMenu.Add(new FloatMenuOption(label, () =>
            {
                HediffDefs.Add(hediffDef);
                RuleParameters = string.Join(",", HediffDefs.Select(def => def.defName));
            }));
        }

        Find.WindowStack.Add(new FloatMenu(hediffMenu));
    }
}