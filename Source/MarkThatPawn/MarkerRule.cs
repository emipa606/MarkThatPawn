using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

public abstract class MarkerRule
{
    public enum AutoRuleType
    {
        Weapon,
        WeaponType,
        Trait,
        Skill,
        Relative
    }

    public bool ConfigError;
    public bool Enabled;
    public string ErrorMessage;
    public MarkerDef MarkerDef;
    public int MarkerIndex;
    public int RuleOrder;
    protected string RuleParameters;
    protected AutoRuleType RuleType;

    public static bool TryGetRuleTypeFromBlob(string blob, out AutoRuleType type)
    {
        type = AutoRuleType.Weapon;
        if (string.IsNullOrEmpty(blob))
        {
            return false;
        }

        var typeString = blob.Split(';')[0];
        switch (typeString)
        {
            case "Weapon":
                type = AutoRuleType.Weapon;
                return true;
            case "WeaponType":
                type = AutoRuleType.WeaponType;
                return true;
            case "Trait":
                type = AutoRuleType.Trait;
                return true;
            case "Skill":
                type = AutoRuleType.Skill;
                return true;
            case "Relative":
                type = AutoRuleType.Relative;
                return true;
            default:
                return false;
        }
    }

    public void ShowChangeMarkerDefMenu()
    {
        var markerSetList = new List<FloatMenuOption>();

        foreach (var def in MarkThatPawn.MarkerDefs)
        {
            markerSetList.Add(new FloatMenuOption(def.LabelCap, () => TrySetMarkerDef(def), def.Icon, Color.white));
        }

        Find.WindowStack.Add(new FloatMenu(markerSetList));
    }

    public void ShowChangeMarkerMenu()
    {
        var markerList = new List<FloatMenuOption>();

        for (var marker = 1; marker <= MarkerDef.MarkerTextures.Count; marker++)
        {
            var localVariable = marker;
            markerList.Add(new FloatMenuOption("MTP.MarkerNumber".Translate(marker),
                () => TrySetMarkerIndex(localVariable - 1), MarkerDef.MarkerTextures[marker - 1], Color.white));
        }

        Find.WindowStack.Add(new FloatMenu(markerList));
    }

    protected virtual bool CanEnable()
    {
        if (MarkerDef == null)
        {
            return false;
        }

        return MarkerIndex < MarkerDef.MarkerTextures.Count;
    }

    public abstract void ShowTypeParametersRect(Rect rect, bool edit);

    public abstract MarkerRule GetCopy();

    public void SaveFromCopy(MarkerRule copy)
    {
        SetBlob(copy.GetBlob());
    }

    protected void SetDefaultValues()
    {
        RuleParameters = string.Empty;
        MarkerIndex = 0;
        MarkerDef = MarkThatPawnMod.instance.Settings.DefaultMarkerSet;
        Enabled = false;
        if (MarkThatPawnMod.instance.Settings.AutoRules?.Any() == true)
        {
            RuleOrder = MarkThatPawnMod.instance.Settings.AutoRules.Max(rule => rule.RuleOrder) + 1;
        }
        else
        {
            RuleOrder = 0;
        }
    }

    protected void SetBlob(string blob)
    {
        SetDefaultValues();
        var rowSplitted = blob.Split(';');
        if (rowSplitted.Length != 6)
        {
            ErrorMessage = "blob is malformed, cannot split into 5 parts";
            ConfigError = true;
            return;
        }

        RuleParameters = rowSplitted[1];

        if (!MarkThatPawn.TryGetMarkerDef(rowSplitted[2], out MarkerDef))
        {
            ErrorMessage = "Cannot parse MarkerDef";
            ConfigError = true;
            return;
        }

        if (!int.TryParse(rowSplitted[3], out MarkerIndex))
        {
            ErrorMessage = "Cannot parse MarkerIndex";
            ConfigError = true;
            return;
        }

        if (!bool.TryParse(rowSplitted[4], out Enabled))
        {
            ErrorMessage = "Cannot parse Enabled";
            ConfigError = true;
        }

        if (!int.TryParse(rowSplitted[5], out var ruleOrder))
        {
            ErrorMessage = "Cannot parse RuleOrder";
            ConfigError = true;
            return;
        }

        RuleOrder = ruleOrder;
        PopulateRuleParameterObjects();
    }

    protected abstract void PopulateRuleParameterObjects();

    public abstract bool AppliesToPawn(Pawn pawn);

    public string GetBlob()
    {
        return $"{RuleType};{RuleParameters};{MarkerDef.defName};{MarkerIndex};{Enabled};{RuleOrder}";
    }

    public string GetTranslatedType()
    {
        return $"MTP.AutomaticType.{RuleType}".Translate();
    }

    public string GetTranslatedMarkerIndex()
    {
        return "MTP.MarkerNumber".Translate(MarkerIndex + 1);
    }

    public Texture2D GetIconTexture()
    {
        if (MarkerDef != null)
        {
            return MarkerDef.MarkerTextures[MarkerIndex];
        }

        ErrorMessage = "There is no MarkerDef defined";
        return BaseContent.BadTex;
    }

    public string GetMarkerBlob()
    {
        return $"{MarkerDef.defName};{MarkerIndex + 1}";
    }

    public void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            if (CanEnable())
            {
                Enabled = true;
            }
            else
            {
                Messages.Message("MTP.CannotEnable".Translate(), MessageTypeDefOf.RejectInput, false);
            }

            return;
        }

        Enabled = false;
    }

    public bool TrySetMarkerDef(MarkerDef markerDef)
    {
        if (markerDef == null)
        {
            ErrorMessage = "MarkerDef cannot be set to a null value";
            return false;
        }

        MarkerDef = markerDef;
        MarkerIndex = 0;
        return true;
    }

    public bool TrySetMarkerIndex(int markerIndex)
    {
        if (MarkerDef == null)
        {
            ErrorMessage = "MarkerDef is not set";
            return false;
        }

        if (MarkerDef.MarkerTextures.Count < MarkerIndex)
        {
            ErrorMessage = "MarkerIndex is higher than the amount of icons";
            return false;
        }

        MarkerIndex = markerIndex;
        return true;
    }

    public void IncreasePrio()
    {
        if (RuleOrder == MarkThatPawnMod.instance.Settings.AutoRules.Min(rule => rule.RuleOrder))
        {
            return;
        }

        var ruleToSwitchWith = MarkThatPawnMod.instance.Settings.AutoRules.OrderByDescending(rule => rule.RuleOrder)
            .First(rule => rule.RuleOrder < RuleOrder);

        (ruleToSwitchWith.RuleOrder, RuleOrder) = (RuleOrder, ruleToSwitchWith.RuleOrder);
    }

    public void DecreasePrio()
    {
        if (RuleOrder == MarkThatPawnMod.instance.Settings.AutoRules.Max(rule => rule.RuleOrder))
        {
            return;
        }

        var ruleToSwitchWith = MarkThatPawnMod.instance.Settings.AutoRules.OrderBy(rule => rule.RuleOrder)
            .First(rule => rule.RuleOrder > RuleOrder);

        (ruleToSwitchWith.RuleOrder, RuleOrder) = (RuleOrder, ruleToSwitchWith.RuleOrder);
    }
}