using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class ApparelMarkerRule : MarkerRule
{
    private List<ThingDef> apparelThingDefs = [];
    private bool or;

    public ApparelMarkerRule()
    {
        RuleType = AutoRuleType.Apparel;
        SetDefaultValues();
    }

    public ApparelMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Apparel;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && apparelThingDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var apparelArea = rect.LeftPart(0.75f);
        if (edit)
        {
            var buttonLabel = "MTP.NoneSelected".Translate();
            if (apparelThingDefs.Any())
            {
                buttonLabel = "MTP.SomeSelected".Translate(apparelThingDefs.Count);
            }

            if (Widgets.ButtonText(apparelArea.TopHalf(), buttonLabel))
            {
                showApparelSelectorMenu();
            }

            TooltipHandler.TipRegion(apparelArea.TopHalf(),
                string.Join("\n", apparelThingDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
            if (apparelThingDefs.Any())
            {
                var originalValue = or;
                Widgets.CheckboxLabeled(apparelArea.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate(),
                    ref or);
                TooltipHandler.TipRegion(apparelArea.BottomHalf().RightHalf().RightPart(0.8f),
                    "MTP.OrLogicTT".Translate());
                if (originalValue != or)
                {
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), apparelThingDefs.Select(thingDef => thingDef.defName).ToArray())}{MarkThatPawn.RuleItemsSplitter}{or}";
                }
            }
        }
        else
        {
            var weaponLabel = "MTP.NoneSelected".Translate();
            if (apparelThingDefs.Any())
            {
                weaponLabel = "MTP.SomeSelected".Translate(apparelThingDefs.Count);
            }

            Widgets.Label(apparelArea.TopHalf(), weaponLabel);
            TooltipHandler.TipRegion(apparelArea.TopHalf(),
                string.Join("\n", apparelThingDefs.Select(thingDef => thingDef.LabelCap).ToArray()));

            if (or)
            {
                Widgets.Label(apparelArea.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate());
                TooltipHandler.TipRegion(apparelArea.BottomHalf().RightHalf().RightPart(0.8f),
                    "MTP.OrLogicTT".Translate());
            }
        }

        if (!apparelThingDefs.Any())
        {
            return;
        }

        var apparelImageRect = rect.RightPartPixels(rect.height).ContractedBy(1f);
        TooltipHandler.TipRegion(apparelImageRect,
            string.Join("\n", apparelThingDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
        GUI.DrawTexture(apparelImageRect, Widgets.GetIconFor(apparelThingDefs.First()));
        if (apparelThingDefs.Count > 1)
        {
            GUI.DrawTexture(apparelImageRect, MarkThatPawn.MultiIconOverlay.mainTexture);
        }
    }

    public override MarkerRule GetCopy()
    {
        return new ApparelMarkerRule(GetBlob());
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

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);

        var apparelPart = ruleParametersSplitted[0];
        apparelThingDefs = [];

        foreach (var apparelDefname in apparelPart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var apparelDef = DefDatabase<ThingDef>.GetNamedSilentFail(apparelDefname);
            if (apparelDef == null)
            {
                ErrorMessage = $"Could not find apparel with defname {apparelDefname}";
                continue;
            }

            apparelThingDefs.Add(apparelDef);
        }

        if (!apparelThingDefs.Any())
        {
            ErrorMessage = $"Could not find apparel based on {apparelPart}, disabling rule";
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

        if (pawn == null || pawn.Destroyed || !pawn.Spawned)
        {
            return false;
        }

        if (pawn.apparel is not { AnyApparel: true })
        {
            return false;
        }

        if (or)
        {
            return pawn.apparel.WornApparel.Any(apparel => apparelThingDefs.Contains(apparel.def));
        }

        var allApparel = pawn.apparel.WornApparel.Select(thing => thing.def);
        return apparelThingDefs.All(def => allApparel.Contains(def));
    }


    private void showApparelSelectorMenu()
    {
        var apparelList = new List<FloatMenuOption>();

        foreach (var apparel in MarkThatPawn.AllValidApparels)
        {
            if (apparelThingDefs.Contains(apparel))
            {
                apparelList.Add(new FloatMenuOption(apparel.LabelCap, () =>
                {
                    apparelThingDefs.Remove(apparel);
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), apparelThingDefs.Select(thingDef => thingDef.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            apparelList.Add(new FloatMenuOption(apparel.LabelCap, () =>
            {
                apparelThingDefs.Add(apparel);
                RuleParameters =
                    $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), apparelThingDefs.Select(thingDef => thingDef.defName))}{MarkThatPawn.RuleItemsSplitter}{or}";
            }));
        }

        Find.WindowStack.Add(new FloatMenu(apparelList));
    }
}