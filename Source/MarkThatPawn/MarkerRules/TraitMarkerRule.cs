using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class TraitMarkerRule : MarkerRule
{
    private bool or;
    private Dictionary<TraitDef, int> traitDefs;

    public TraitMarkerRule()
    {
        RuleType = AutoRuleType.Trait;
        traitDefs = new Dictionary<TraitDef, int>();
        SetDefaultValues();
    }

    public TraitMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Trait;
        traitDefs = new Dictionary<TraitDef, int>();
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && traitDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var traitRect = rect.TopHalf();
        if (edit)
        {
            if (Widgets.ButtonText(traitRect,
                    !traitDefs.Any() ? "MTP.NoneSelected".Translate() : "MTP.SomeSelected".Translate(traitDefs.Count)))
            {
                showTraitSelectorMenu();
            }

            TooltipHandler.TipRegion(traitRect,
                string.Join(", ", traitDefs.Select(pair => pair.Key.DataAtDegree(pair.Value).LabelCap)));

            var originalValue = or;
            Widgets.CheckboxLabeled(rect.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate(),
                ref or);
            TooltipHandler.TipRegion(rect.BottomHalf().RightHalf().RightPart(0.8f),
                "MTP.OrLogicTT".Translate());
            if (originalValue != or)
            {
                RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                    traitDefs.Select(pair => $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
            }

            return;
        }

        if (!traitDefs.Any())
        {
            Widgets.Label(rect, "MTP.NoneSelected".Translate());
            return;
        }

        Widgets.Label(traitRect, "MTP.SomeSelected".Translate(traitDefs.Count));
        TooltipHandler.TipRegion(traitRect,
            string.Join(", ", traitDefs.Select(pair => pair.Key.DataAtDegree(pair.Value).LabelCap)));

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
        return new TraitMarkerRule(GetBlob());
    }

    protected override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        traitDefs = new Dictionary<TraitDef, int>();

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
        var traitPart = ruleParametersSplitted[0];

        foreach (var traitKeyPair in traitPart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var traitSplitted = traitKeyPair.Split(MarkThatPawn.RuleInternalSplitter);

            if (traitSplitted.Length != 2)
            {
                ConfigError = true;
                continue;
            }

            var traitDef =
                DefDatabase<TraitDef>.GetNamedSilentFail(traitSplitted[0]);
            if (traitDef == null)
            {
                ConfigError = true;
                continue;
            }

            if (!int.TryParse(traitSplitted[1], out var traitDegree))
            {
                ConfigError = true;
                continue;
            }

            if (!traitDef.degreeDatas.Any(data => data.degree == traitDegree))
            {
                ConfigError = true;
                continue;
            }

            traitDefs[traitDef] = traitDegree;
        }

        if (ConfigError)
        {
            ErrorMessage = $"Could not parse all traitDefs from {RuleParameters}, disabling rule";
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

        var pawnTraits = pawn.story?.traits?.allTraits;

        if (pawnTraits == null)
        {
            return false;
        }

        return or
            ? pawnTraits.Any(trait => traitDefs.Any(pair => pair.Key == trait.def && pair.Value == trait.Degree))
            : traitDefs.All(traitDef =>
                pawnTraits.Any(trait => trait.def == traitDef.Key && trait.Degree == traitDef.Value));
    }

    private void showTraitSelectorMenu()
    {
        var traitMenu = new List<FloatMenuOption>();

        foreach (var traitDef in MarkThatPawn.AllTraits)
        {
            foreach (var traitDefDegree in traitDef.degreeDatas)
            {
                if (traitDefs.Any(pair => pair.Key == traitDef && pair.Value == traitDefDegree.degree))
                {
                    traitMenu.Add(new FloatMenuOption(traitDefDegree.LabelCap, () =>
                    {
                        traitDefs.Remove(traitDef);
                        RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                            traitDefs.Select(pair =>
                                $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                    }, MarkThatPawn.RemoveIcon, Color.white));
                    continue;
                }

                traitMenu.Add(new FloatMenuOption(traitDefDegree.LabelCap, () =>
                {
                    traitDefs[traitDef] = traitDefDegree.degree;
                    RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                        traitDefs.Select(pair => $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                }));
            }
        }

        Find.WindowStack.Add(new FloatMenu(traitMenu));
    }
}