using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class SkillMarkerRule : MarkerRule
{
    private readonly Dictionary<SkillDef, Passion> passionDefs;
    private bool or;
    private Dictionary<SkillDef, int> skillDefs;

    public SkillMarkerRule()
    {
        RuleType = AutoRuleType.Skill;
        skillDefs = new Dictionary<SkillDef, int>();
        passionDefs = new Dictionary<SkillDef, Passion>();
        SetDefaultValues();
    }

    public SkillMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Skill;
        skillDefs = new Dictionary<SkillDef, int>();
        passionDefs = new Dictionary<SkillDef, Passion>();
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && skillDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var skillRect = rect.TopHalf();

        var labelList = new List<string>();
        foreach (var skillDef in skillDefs)
        {
            var label = $"{skillDef.Key.LabelCap}: {skillDef.Value}";
            if (passionDefs.TryGetValue(skillDef.Key, out var passionDef))
            {
                switch (passionDef)
                {
                    case Passion.Minor:
                        label += "*";
                        break;
                    case Passion.Major:
                        label += "**";
                        break;
                }
            }

            labelList.Add(label);
        }

        var skillLabel = string.Join("\n", labelList);

        if (edit)
        {
            if (Widgets.ButtonText(skillRect,
                    !skillDefs.Any() ? "MTP.NoneSelected".Translate() : "MTP.SomeSelected".Translate(skillDefs.Count)))
            {
                showSkillSelectorMenu();
            }

            TooltipHandler.TipRegion(skillRect, skillLabel);

            var originalValue = or;
            Widgets.CheckboxLabeled(rect.BottomHalf().RightHalf().RightPart(0.8f), "MTP.OrLogic".Translate(),
                ref or);
            TooltipHandler.TipRegion(rect.BottomHalf().RightHalf().RightPart(0.8f),
                "MTP.OrLogicTT".Translate());
            if (originalValue != or)
            {
                RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                    skillDefs.Select(pair =>
                        $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}{MarkThatPawn.RuleInternalSplitter}{passionDefs[pair.Key].ToString()}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
            }

            return;
        }

        if (!skillDefs.Any())
        {
            Widgets.Label(skillRect, "MTP.NoneSelected".Translate());
            return;
        }

        Widgets.Label(skillRect, "MTP.SomeSelected".Translate(skillDefs.Count));
        TooltipHandler.TipRegion(skillRect, skillLabel);

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
        return new SkillMarkerRule(GetBlob());
    }

    protected override void PopulateRuleParameterObjects()
    {
        if (RuleParameters == null)
        {
            return;
        }

        skillDefs = new Dictionary<SkillDef, int>();

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        if (!RuleParameters.Contains(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            RuleParameters = RuleParameters.Replace(',', MarkThatPawn.RuleAlternateItemsSplitter);
            RuleParameters = RuleParameters.Replace(MarkThatPawn.RuleItemsSplitter, MarkThatPawn.RuleInternalSplitter);
        }

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);
        var skillPart = ruleParametersSplitted[0];

        foreach (var skillKeyPair in skillPart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            if (!skillKeyPair.Contains(MarkThatPawn.RuleInternalSplitter) ||
                skillKeyPair.Split(MarkThatPawn.RuleInternalSplitter).Length != 2 &&
                skillKeyPair.Split(MarkThatPawn.RuleInternalSplitter).Length != 3)
            {
                ConfigError = true;
                continue;
            }

            var skillDef =
                DefDatabase<SkillDef>.GetNamedSilentFail(
                    skillKeyPair.Split(MarkThatPawn.RuleInternalSplitter)[0]);
            if (skillDef == null)
            {
                ConfigError = true;
                continue;
            }

            if (!int.TryParse(skillKeyPair.Split(MarkThatPawn.RuleInternalSplitter)[1], out var skillLevel))
            {
                ConfigError = true;
                continue;
            }

            skillDefs[skillDef] = skillLevel;

            if (skillKeyPair.Split(MarkThatPawn.RuleInternalSplitter).Length == 3)
            {
                if (!Enum.TryParse(skillKeyPair.Split(MarkThatPawn.RuleInternalSplitter)[2], out Passion passion))
                {
                    passionDefs[skillDef] = Passion.None;
                    ConfigError = true;
                    continue;
                }

                passionDefs[skillDef] = passion;
            }
            else
            {
                passionDefs[skillDef] = Passion.None;
            }
        }

        if (ConfigError)
        {
            ErrorMessage = $"Could not parse all skillDefs from {RuleParameters}, disabling rule";
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

        var pawnSkills = pawn.skills?.skills;

        if (pawnSkills == null)
        {
            return false;
        }

        return or
            ? pawnSkills.Any(skillRecord =>
                skillDefs.Any(pair =>
                    pair.Key == skillRecord.def && pair.Value <= skillRecord.Level &&
                    skillRecord.passion >= passionDefs[pair.Key]))
            : skillDefs.All(skillDef =>
                pawnSkills.Any(skillRecord => skillRecord.def == skillDef.Key && skillRecord.Level >= skillDef.Value &&
                                              skillRecord.passion >= passionDefs[skillDef.Key]));
    }

    private void showSkillSelectorMenu()
    {
        var skillMenu = new List<FloatMenuOption>();

        foreach (var skillDef in MarkThatPawn.AllSkills)
        {
            if (skillDefs.ContainsKey(skillDef))
            {
                skillMenu.Add(new FloatMenuOption(skillDef.LabelCap, () =>
                {
                    skillDefs.Remove(skillDef);
                    passionDefs.Remove(skillDef);
                    RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                        skillDefs.Select(pair =>
                            $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}{MarkThatPawn.RuleInternalSplitter}{passionDefs[pair.Key].ToString()}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                }, TexButton.Empty, Color.white));
                continue;
            }

            skillMenu.Add(new FloatMenuOption(skillDef.LabelCap, () =>
            {
                var subMenu = new List<FloatMenuOption>();
                for (var i = 1; i < 21; i++)
                {
                    var level = i;
                    subMenu.Add(new FloatMenuOption($"{skillDef.LabelCap} {i}", () =>
                    {
                        var subSubMenu = new List<FloatMenuOption>
                        {
                            new FloatMenuOption("PassionNone".Translate(), () =>
                            {
                                skillDefs[skillDef] = level;
                                passionDefs[skillDef] = Passion.None;
                                RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                                    skillDefs.Select(pair =>
                                        $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}{MarkThatPawn.RuleInternalSplitter}{passionDefs[pair.Key].ToString()}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                            }),
                            new FloatMenuOption("PassionMinor".Translate(), () =>
                            {
                                skillDefs[skillDef] = level;
                                passionDefs[skillDef] = Passion.Minor;
                                RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                                    skillDefs.Select(pair =>
                                        $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}{MarkThatPawn.RuleInternalSplitter}{passionDefs[pair.Key].ToString()}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                            }, SkillUI.PassionMinorIcon, Color.white),
                            new FloatMenuOption("PassionMajor".Translate(), () =>
                            {
                                skillDefs[skillDef] = level;
                                passionDefs[skillDef] = Passion.Major;
                                RuleParameters = $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(),
                                    skillDefs.Select(pair =>
                                        $"{pair.Key.defName}{MarkThatPawn.RuleInternalSplitter}{pair.Value}{MarkThatPawn.RuleInternalSplitter}{passionDefs[pair.Key].ToString()}"))}{MarkThatPawn.RuleItemsSplitter}{or}";
                            }, SkillUI.PassionMajorIcon, Color.white)
                        };
                        Find.WindowStack.Add(new FloatMenu(subSubMenu));
                    }, TexUI.ArrowTexRight, Color.white, iconJustification: HorizontalJustification.Right));
                }

                Find.WindowStack.Add(new FloatMenu(subMenu));
            }, TexUI.ArrowTexRight, Color.white, iconJustification: HorizontalJustification.Right));
        }

        Find.WindowStack.Add(new FloatMenu(skillMenu));
    }
}