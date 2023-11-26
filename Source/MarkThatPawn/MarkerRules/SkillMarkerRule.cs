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
        var skillListRect = rect;

        if (edit)
        {
            skillListRect = rect.RightPart(0.75f);
            var buttonRect = rect.LeftPart(0.23f);
            if (Widgets.ButtonText(buttonRect,
                    !skillDefs.Any() ? "MTP.NoneSelected".Translate() : "MTP.SomeSelected".Translate(skillDefs.Count)))
            {
                showSkillSelectorMenu();
            }
        }

        if (!skillDefs.Any())
        {
            Widgets.Label(skillListRect, "MTP.NoneSelected".Translate());
            return;
        }

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

        Widgets.Label(skillListRect, string.Join(", ", labelList));
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

        foreach (var skillKeyPair in RuleParameters.Split(','))
        {
            if (!skillKeyPair.Contains("|") ||
                skillKeyPair.Split('|').Length != 2 && skillKeyPair.Split('|').Length != 3)
            {
                ConfigError = true;
                continue;
            }

            var skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skillKeyPair.Split('|')[0]);
            if (skillDef == null)
            {
                ConfigError = true;
                continue;
            }

            if (!int.TryParse(skillKeyPair.Split('|')[1], out var skillLevel))
            {
                ConfigError = true;
                continue;
            }

            skillDefs[skillDef] = skillLevel;

            if (skillKeyPair.Split('|').Length == 3)
            {
                if (!Enum.TryParse(skillKeyPair.Split('|')[2], out Passion passion))
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

        foreach (var skillDef in skillDefs)
        {
            if (!pawnSkills.Any(skill =>
                    skill.def == skillDef.Key && skill.Level >= skillDef.Value &&
                    skill.passion >= passionDefs[skillDef.Key]))
            {
                return false;
            }
        }

        return true;
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
                    RuleParameters = string.Join(",",
                        skillDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}|{passionDefs[pair.Key]}"));
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
                                RuleParameters = string.Join(",",
                                    skillDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}|None"));
                            }),
                            new FloatMenuOption("PassionMinor".Translate(), () =>
                            {
                                skillDefs[skillDef] = level;
                                passionDefs[skillDef] = Passion.Minor;
                                RuleParameters = string.Join(",",
                                    skillDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}|Minor"));
                            }, SkillUI.PassionMinorIcon, Color.white),
                            new FloatMenuOption("PassionMajor".Translate(), () =>
                            {
                                skillDefs[skillDef] = level;
                                passionDefs[skillDef] = Passion.Major;
                                RuleParameters = string.Join(",",
                                    skillDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}|Major"));
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