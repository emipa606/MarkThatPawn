using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class GeneMarkerRule : MarkerRule
{
    private GeneDef RuleGene;

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
        return base.CanEnable() && ModLister.BiotechInstalled && RuleGene != null;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var geneArea = rect.LeftPart(0.75f).TopHalf().CenteredOnYIn(rect);
        if (edit)
        {
            if (Widgets.ButtonText(geneArea, RuleGene?.LabelCap ?? "MTP.NoneSelected".Translate()))
            {
                showGeneSelectorMenu();
            }
        }
        else
        {
            Widgets.Label(geneArea, RuleGene?.LabelCap ?? "MTP.NoneSelected".Translate());
        }

        if (RuleGene == null)
        {
            return;
        }

        var geneImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(geneImageRect, RuleGene.description);
        GUI.DrawTexture(geneImageRect, RuleGene.Icon);
    }

    public override MarkerRule GetCopy()
    {
        return new GeneMarkerRule(GetBlob());
    }

    protected override void PopulateRuleParameterObjects()
    {
        if (!ModLister.BiotechInstalled)
        {
            ErrorMessage = "Biotech missing, disabling rule";
            ConfigError = true;
            return;
        }

        if (RuleParameters == null)
        {
            return;
        }

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        RuleGene = DefDatabase<GeneDef>.GetNamedSilentFail(RuleParameters);
        if (RuleGene != null)
        {
            return;
        }

        ErrorMessage = $"Could not find Gene with defname {RuleParameters}, disabling rule";
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

        return pawn.genes?.GenesListForReading?.Any(gene => gene.def == RuleGene) == true;
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
                    geneCategoryList.Add(new FloatMenuOption(gene.LabelCap, () =>
                    {
                        RuleParameters = gene.defName;
                        RuleGene = gene;
                    }, gene.Icon, gene.IconColor));
                }

                Find.WindowStack.Add(new FloatMenu(geneCategoryList));
            }, TexUI.ArrowTexRight, Color.white, iconJustification: HorizontalJustification.Right));
        }

        var allUndefinedGenes = MarkThatPawn.AllValidGenes.Where(def => def.displayCategory == null);
        if (allUndefinedGenes.Any())
        {
            geneList.Add(new FloatMenuOption("MTP.AutomaticType.GeneUndefinedCategory".Translate(), () =>
            {
                var geneCategoryList = new List<FloatMenuOption>();
                foreach (var gene in allUndefinedGenes.OrderBy(def => def.label))
                {
                    geneCategoryList.Add(new FloatMenuOption(gene.LabelCap, () =>
                    {
                        RuleParameters = gene.defName;
                        RuleGene = gene;
                    }, gene.Icon, gene.IconColor));
                }

                Find.WindowStack.Add(new FloatMenu(geneCategoryList));
            }, TexUI.ArrowTexRight, Color.white, iconJustification: HorizontalJustification.Right));
        }

        Find.WindowStack.Add(new FloatMenu(geneList));
    }
}