using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

public class SkillMarkerRule : MarkerRule
{
    private Dictionary<SkillDef, int> skillDefs;

    public SkillMarkerRule()
    {
        RuleType = AutoRuleType.Skill;
        skillDefs = new Dictionary<SkillDef, int>();
        SetDefaultValues();
    }

    public SkillMarkerRule(string blob)
    {
        RuleType = AutoRuleType.Skill;
        skillDefs = new Dictionary<SkillDef, int>();
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
            skillListRect = rect.RightPart(0.8f);
            var buttonRect = rect.LeftPart(0.18f);
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

        Widgets.Label(skillListRect,
            string.Join(", ", skillDefs.Select(pair => $"{pair.Key.LabelCap}: {pair.Value}")));
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
        foreach (var skillKeyPair in RuleParameters.Split(','))
        {
            if (!skillKeyPair.Contains("|") || skillKeyPair.Split('|').Length != 2)
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
        }

        if (ConfigError)
        {
            ErrorMessage = $"Could not parse all skillDefs from {RuleParameters}, disabling rule";
        }
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
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
            if (!pawnSkills.Any(skill => skill.def == skillDef.Key && skill.Level >= skillDef.Value))
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
                    RuleParameters = string.Join(",", skillDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}"));
                }, TexButton.Minus, Color.white));
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
                        skillDefs[skillDef] = level;
                        RuleParameters = string.Join(",", skillDefs.Select(pair => $"{pair.Key.defName}|{pair.Value}"));
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(subMenu));
            }));
        }

        Find.WindowStack.Add(new FloatMenu(skillMenu));
    }
}