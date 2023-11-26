using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class TraitMarkerRule : MarkerRule
{
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
        var traitListRect = rect;

        if (edit)
        {
            traitListRect = rect.RightPart(0.75f);
            var buttonRect = rect.LeftPart(0.23f);
            if (Widgets.ButtonText(buttonRect,
                    !traitDefs.Any() ? "MTP.NoneSelected".Translate() : "MTP.SomeSelected".Translate(traitDefs.Count)))
            {
                showTraitSelectorMenu();
            }
        }

        if (!traitDefs.Any())
        {
            Widgets.Label(traitListRect, "MTP.NoneSelected".Translate());
            return;
        }

        Widgets.Label(traitListRect,
            string.Join(", ", traitDefs.Select(pair => pair.Key.DataAtDegree(pair.Value).LabelCap)));
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

        foreach (var traitKeyPair in RuleParameters.Split(','))
        {
            if (!traitKeyPair.Contains("|") || traitKeyPair.Split('|').Length != 2)
            {
                ConfigError = true;
                continue;
            }

            var traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(traitKeyPair.Split('|')[0]);
            if (traitDef == null)
            {
                ConfigError = true;
                continue;
            }

            if (!int.TryParse(traitKeyPair.Split('|')[1], out var traitDegree))
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

        foreach (var traitDef in traitDefs)
        {
            if (!pawnTraits.Any(trait => trait.def == traitDef.Key && trait.Degree == traitDef.Value))
            {
                return false;
            }
        }

        return true;
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
                        RuleParameters = string.Join(",", traitDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}"));
                    }, MarkThatPawn.RemoveIcon, Color.white));
                    continue;
                }

                traitMenu.Add(new FloatMenuOption(traitDefDegree.LabelCap, () =>
                {
                    traitDefs[traitDef] = traitDefDegree.degree;
                    RuleParameters = string.Join(",", traitDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}"));
                }));
            }
        }

        Find.WindowStack.Add(new FloatMenu(traitMenu));
    }
}