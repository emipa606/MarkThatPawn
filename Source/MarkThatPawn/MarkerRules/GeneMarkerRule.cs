using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class GeneMarkerRule : MarkerRule
{
    private List<GeneDef> geneDefs = [];
    private bool or;

    public GeneMarkerRule()
    {
        RuleType = AutoRuleType.Gene;
        SetDefaultValues();
    }

    public GeneMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Gene;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && ModLister.BiotechInstalled && geneDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var geneArea = rect.LeftPart(0.75f);
        if (edit)
        {
            var buttonLabel = "MTP.NoneSelected".Translate();
            if (geneDefs.Any())
            {
                buttonLabel = "MTP.SomeSelected".Translate(geneDefs.Count);
            }

            if (Widgets.ButtonText(geneArea.TopHalf(), buttonLabel))
            {
                showGeneSelectorMenu();
            }

            TooltipHandler.TipRegion(geneArea.TopHalf(),
                string.Join("\n", geneDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
            if (geneDefs.Any())
            {
                var originalValue = or;
                Widgets.CheckboxLabeled(geneArea.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate(),
                    ref or);
                TooltipHandler.TipRegion(geneArea.BottomHalf().RightHalf().RightPart(0.8f),
                    "MTP.OrLogicTT".Translate());
                if (originalValue != or)
                {
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), geneDefs.Select(geneDef => geneDef.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
                }
            }
        }
        else
        {
            var weaponLabel = "MTP.NoneSelected".Translate();
            if (geneDefs.Any())
            {
                weaponLabel = "MTP.SomeSelected".Translate(geneDefs.Count);
            }

            Widgets.Label(geneArea.TopHalf(), weaponLabel);
            TooltipHandler.TipRegion(geneArea.TopHalf(),
                string.Join("\n", geneDefs.Select(thingDef => thingDef.LabelCap).ToArray()));

            if (or)
            {
                Widgets.Label(geneArea.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate());
                TooltipHandler.TipRegion(geneArea.BottomHalf().RightHalf().RightPart(0.8f),
                    "MTP.OrLogicTT".Translate());
            }
        }

        if (!geneDefs.Any())
        {
            return;
        }

        var geneImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(geneImageRect,
            string.Join("\n", geneDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
        GUI.DrawTexture(geneImageRect, geneDefs.First().Icon);
        if (geneDefs.Count > 1)
        {
            GUI.DrawTexture(geneImageRect, MarkThatPawn.MultiIconOverlay.mainTexture);
        }
    }

    public override MarkerRule GetCopy()
    {
        return new GeneMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        if (!ModLister.BiotechInstalled)
        {
            ErrorMessage = "Biotech missing, disabling rule";
            ConfigError = true;
            return;
        }

        switch (RuleParameters)
        {
            case null:
            case "" when !Enabled:
                return;
        }

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);

        var genePart = ruleParametersSplitted[0];
        geneDefs = [];

        foreach (var geneDefName in genePart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
            if (geneDef == null)
            {
                ErrorMessage = $"Could not find gene with defname {geneDefName}";
                continue;
            }

            geneDefs.Add(geneDef);
        }

        if (!geneDefs.Any())
        {
            ErrorMessage = $"Could not find Genes from {RuleParameters}, disabling rule";
            ConfigError = true;
            return;
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

        if (pawn.genes == null)
        {
            return false;
        }

        if (or)
        {
            return pawn.genes.GenesListForReading.Any(gene => geneDefs.Contains(gene.def));
        }

        var allGenes = pawn.genes.GenesListForReading.Select(gene => gene.def);
        return geneDefs.All(def => allGenes.Contains(def));
    }


    private void showGeneSelectorMenu()
    {
        var geneList = new List<FloatMenuOption>();

        foreach (var geneCategory in MarkThatPawn.AllValidGeneCategories)
        {
            geneList.Add(new FloatMenuOption(geneCategory.LabelCap, () =>
            {
                var geneCategoryList = new List<FloatMenuOption>();
                foreach (var gene in MarkThatPawn.AllValidGenes.Where(def => def.displayCategory == geneCategory)
                             .OrderBy(def => def.label))
                {
                    if (geneDefs.Contains(gene))
                    {
                        geneCategoryList.Add(new FloatMenuOption(gene.LabelCap, () =>
                        {
                            geneDefs.Remove(gene);
                            RuleParameters =
                                $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), geneDefs.Select(geneDef => geneDef.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
                        }, MarkThatPawn.RemoveIcon, Color.white));
                        continue;
                    }

                    geneCategoryList.Add(new FloatMenuOption(gene.LabelCap, () =>
                    {
                        geneDefs.Add(gene);
                        RuleParameters =
                            $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), geneDefs.Select(geneDef => geneDef.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
                    }, gene.Icon, gene.IconColor));
                }

                Find.WindowStack.Add(new FloatMenu(geneCategoryList));
            }, TexUI.ArrowTexRight, Color.white, iconJustification: HorizontalJustification.Right));
        }

        var allUndefinedGenes = MarkThatPawn.AllValidGenes.Where(def => def.displayCategory == null);
        var undefinedGenes = allUndefinedGenes as GeneDef[] ?? allUndefinedGenes.ToArray();
        if (undefinedGenes.Any())
        {
            geneList.Add(new FloatMenuOption("MTP.AutomaticType.GeneUndefinedCategory".Translate(), () =>
            {
                var geneCategoryList = new List<FloatMenuOption>();
                foreach (var gene in undefinedGenes.OrderBy(def => def.label))
                {
                    if (geneDefs.Contains(gene))
                    {
                        geneCategoryList.Add(new FloatMenuOption(gene.LabelCap, () =>
                        {
                            geneDefs.Remove(gene);
                            RuleParameters =
                                $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), geneDefs.Select(geneDef => geneDef.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
                        }, MarkThatPawn.RemoveIcon, Color.white));
                        continue;
                    }

                    geneCategoryList.Add(new FloatMenuOption(gene.LabelCap, () =>
                    {
                        geneDefs.Add(gene);
                        RuleParameters =
                            $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), geneDefs.Select(geneDef => geneDef.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
                    }, gene.Icon, gene.IconColor));
                }

                Find.WindowStack.Add(new FloatMenu(geneCategoryList));
            }, TexUI.ArrowTexRight, Color.white, iconJustification: HorizontalJustification.Right));
        }

        Find.WindowStack.Add(new FloatMenu(geneList));
    }
}