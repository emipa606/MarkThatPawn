using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class IdeologyRoleMarkerRule : MarkerRule
{
    private List<PreceptDef> ideoRoleDefs = [];

    public IdeologyRoleMarkerRule()
    {
        RuleType = AutoRuleType.IdeologyRole;
        SetDefaultValues();
    }

    public IdeologyRoleMarkerRule(string blob)
    {
        RuleType = AutoRuleType.IdeologyRole;
        SetBlob(blob);
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && ideoRoleDefs?.Any() == true;
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var ideoRoleArea = rect.LeftPart(0.75f);
        if (edit)
        {
            var buttonLabel = "MTP.NoneSelected".Translate();
            if (ideoRoleDefs.Any())
            {
                buttonLabel = "MTP.SomeSelected".Translate(ideoRoleDefs.Count);
            }

            if (Widgets.ButtonText(ideoRoleArea.TopHalf(), buttonLabel))
            {
                showIdeoRoleSelectorMenu();
            }

            TooltipHandler.TipRegion(ideoRoleArea.TopHalf(),
                string.Join("\n", ideoRoleDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
        }
        else
        {
            var weaponLabel = "MTP.NoneSelected".Translate();
            if (ideoRoleDefs.Any())
            {
                weaponLabel = "MTP.SomeSelected".Translate(ideoRoleDefs.Count);
            }

            Widgets.Label(ideoRoleArea.TopHalf(), weaponLabel);
            TooltipHandler.TipRegion(ideoRoleArea.TopHalf(),
                string.Join("\n", ideoRoleDefs.Select(thingDef => thingDef.LabelCap).ToArray()));
        }
    }

    public override MarkerRule GetCopy()
    {
        return new IdeologyRoleMarkerRule(GetBlob());
    }

    public override void PopulateRuleParameterObjects()
    {
        switch (RuleParameters)
        {
            case null:
            case "" when !Enabled:
                return;
        }

        var ruleParametersSplitted = RuleParameters.Split(MarkThatPawn.RuleItemsSplitter);

        var ideoRolePart = ruleParametersSplitted[0];
        ideoRoleDefs = [];

        foreach (var ideoRoleDefName in ideoRolePart.Split(MarkThatPawn.RuleAlternateItemsSplitter))
        {
            var ideoRoleDef = DefDatabase<PreceptDef>.GetNamedSilentFail(ideoRoleDefName);
            if (ideoRoleDef == null)
            {
                ErrorMessage = $"Could not find ideoRole with defname {ideoRoleDefName}";
                continue;
            }

            ideoRoleDefs.Add(ideoRoleDef);
        }

        if (ideoRoleDefs.Any())
        {
            return;
        }

        ErrorMessage = $"Could not find ideoRole based on {ideoRolePart}, disabling rule";
        ConfigError = true;
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        return base.AppliesToPawn(pawn) && ideoRoleDefs.Any(def => pawn.Ideo?.GetRole(pawn)?.def == def);
    }

    private void showIdeoRoleSelectorMenu()
    {
        var ideoRoleList = new List<FloatMenuOption>();

        foreach (var ideoRole in MarkThatPawn.AllIdeologyRoles)
        {
            if (ideoRoleDefs.Contains(ideoRole))
            {
                ideoRoleList.Add(new FloatMenuOption(ideoRole.LabelCap, () =>
                {
                    ideoRoleDefs.Remove(ideoRole);
                    RuleParameters =
                        $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), ideoRoleDefs.Select(thingDef => thingDef.defName))}";
                }, MarkThatPawn.RemoveIcon, Color.white));
                continue;
            }

            ideoRoleList.Add(new FloatMenuOption(ideoRole.LabelCap, () =>
            {
                ideoRoleDefs.Add(ideoRole);
                RuleParameters =
                    $"{string.Join(MarkThatPawn.RuleAlternateItemsSplitter.ToString(), ideoRoleDefs.Select(thingDef => thingDef.defName))}";
            }));
        }

        Find.WindowStack.Add(new FloatMenu(ideoRoleList));
    }
}