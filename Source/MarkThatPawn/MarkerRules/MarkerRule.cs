using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using static MarkThatPawn.MarkThatPawn;

namespace MarkThatPawn.MarkerRules;

public abstract class MarkerRule
{
    public enum AutoRuleType
    {
        Drafted,
        Downed,
        MentalState,
        HediffDynamic,
        Weapon,
        WeaponType,
        Apparel,
        ApparelType,
        Trait,
        Skill,
        Relative,
        FactionLeader,
        Guest,
        PawnType,
        Animal,
        Mechanoid,
        HediffStatic,
        AnyHediffStatic,
        Gender,
        Age,
        Gene,
        Xenotype,
        FactionIcon,
        IdeologyIcon,
        IdeologyRole,
        TDFindLib,
        Title
    }

    public List<PawnType> ApplicablePawnTypes;

    public bool ConfigError;
    public bool Enabled;
    public string ErrorMessage;
    public bool IsInCorrectGame;
    public bool IsOverride;
    public MarkerDef MarkerDef;
    public int MarkerIndex;
    public PawnType PawnLimitation;
    public string RawBlob;
    public bool RequiresASpecificGame;
    public int RuleOrder;
    protected string RuleParameters;
    protected AutoRuleType RuleType;
    public bool UsesDynamicIcons;

    public static bool TryGetRuleTypeFromBlob(string blob, out AutoRuleType type)
    {
        type = AutoRuleType.Weapon;
        if (string.IsNullOrEmpty(blob))
        {
            return false;
        }

        var typeString = blob.Split(BlobSplitter)[0];
        return Enum.TryParse(typeString, out type);
    }

    public void ShowChangeMarkerDefMenu()
    {
        if (ConfigError)
        {
            return;
        }

        var markerSetList = new List<FloatMenuOption>();

        foreach (var def in MarkerDefs)
        {
            markerSetList.Add(new FloatMenuOption(def.LabelCap, () => TrySetMarkerDef(def), def.Icon, Color.white));
        }

        Find.WindowStack.Add(new FloatMenu(markerSetList));
    }

    public void ShowChangeMarkerMenu()
    {
        if (ConfigError)
        {
            return;
        }

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
        if (ConfigError)
        {
            return false;
        }

        if (MarkerDef == null)
        {
            return false;
        }

        return MarkerIndex < MarkerDef.MarkerTextures.Count;
    }

    public virtual void ShowTypeParametersRect(Rect rect, bool edit)
    {
    }

    public abstract MarkerRule GetCopy();

    public virtual MarkerRule GetEditableVersion()
    {
        return GetCopy();
    }

    public void SaveFromCopy(MarkerRule copy)
    {
        SetBlob(copy.GetBlob());
    }

    protected void SetDefaultValues()
    {
        if (ConfigError)
        {
            return;
        }

        RuleParameters = string.Empty;
        PawnLimitation = PawnType.Default;
        MarkerIndex = 0;
        MarkerDef = MarkThatPawnMod.instance.Settings.DefaultMarkerSet;
        Enabled = false;
        ApplicablePawnTypes = Enum.GetValues(typeof(PawnType)).Cast<PawnType>().ToList();
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
        RawBlob = blob;

        SetDefaultValues();
        var rowSplitted = blob.Split(BlobSplitter);
        if (rowSplitted.Length is < 6 or > 8)
        {
            ErrorMessage = $"Blob is malformed, cannot split into correct parts, got {rowSplitted.Length}";
            ConfigError = true;
            return;
        }

        if (rowSplitted.Length == 6)
        {
            rowSplitted = $"{blob}{BlobSplitter}Default".Split(BlobSplitter);
        }

        RuleParameters = rowSplitted[1];

        if (!TryGetMarkerDef(rowSplitted[2], out MarkerDef))
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

        if (!Enum.TryParse(rowSplitted[6], out PawnType pawnType))
        {
            ErrorMessage = "Cannot parse PawnLimitation";
            ConfigError = true;
            return;
        }

        PawnLimitation = pawnType;
        RuleOrder = ruleOrder;
        PopulateRuleParameterObjects();
    }

    public virtual void PopulateRuleParameterObjects()
    {
    }

    public virtual void OnDelete()
    {
    }

    public virtual bool AppliesToPawn(Pawn pawn)
    {
        if (pawn.Dead)
        {
            if (!MarkThatPawnMod.instance.Settings.ShowOnCorpses)
            {
                return false;
            }
        }
        else
        {
            if (pawn is not { Spawned: true })
            {
                return false;
            }
        }

        return PawnLimitation == PawnType.Default || pawn.GetPawnTypes().Contains(PawnLimitation);
    }

    public string GetBlob()
    {
        return ConfigError
            ? RawBlob
            : $"{RuleType};{RuleParameters};{MarkerDef.defName};{MarkerIndex};{Enabled};{RuleOrder};{PawnLimitation}";
    }

    public string GetTranslatedType()
    {
        return $"MTP.AutomaticType.{RuleType}".Translate();
    }

    public string GetTranslatedMarkerIndex()
    {
        return "MTP.MarkerNumber".Translate(MarkerIndex + 1);
    }

    public virtual Texture2D GetIconTexture()
    {
        if (ConfigError)
        {
            return BaseContent.BadTex;
        }

        if (MarkerDef != null)
        {
            return MarkerDef.MarkerTextures[MarkerIndex];
        }

        ErrorMessage = "There is no MarkerDef defined";
        return BaseContent.BadTex;
    }

    public virtual string GetMarkerBlob()
    {
        return ConfigError ? "" : $"{MarkerDef.defName};{MarkerIndex + 1}";
    }

    public void SetEnabled(bool enabled)
    {
        if (ConfigError)
        {
            return;
        }

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

    public void TrySetMarkerDef(MarkerDef markerDef)
    {
        if (ConfigError)
        {
            return;
        }

        if (markerDef == null)
        {
            ErrorMessage = "MarkerDef cannot be set to a null value";
            return;
        }

        MarkerDef = markerDef;
        MarkerIndex = 0;
    }

    public void TrySetMarkerIndex(int markerIndex)
    {
        if (ConfigError)
        {
            return;
        }

        if (MarkerDef == null)
        {
            ErrorMessage = "MarkerDef is not set";
            return;
        }

        if (MarkerDef.MarkerTextures.Count < MarkerIndex)
        {
            ErrorMessage = "MarkerIndex is higher than the amount of icons";
            return;
        }

        MarkerIndex = markerIndex;
    }

    public void IncreasePrio()
    {
        if (ConfigError)
        {
            return;
        }

        if (RuleOrder == MarkThatPawnMod.instance.Settings.AutoRules.Where(rule => rule.IsOverride == IsOverride)
                .Min(rule => rule.RuleOrder))
        {
            return;
        }

        var ruleToSwitchWith = MarkThatPawnMod.instance.Settings.AutoRules.Where(rule => rule.IsOverride == IsOverride)
            .OrderByDescending(rule => rule.RuleOrder)
            .First(rule => rule.RuleOrder < RuleOrder);

        (ruleToSwitchWith.RuleOrder, RuleOrder) = (RuleOrder, ruleToSwitchWith.RuleOrder);
    }

    public void DecreasePrio()
    {
        if (ConfigError)
        {
            return;
        }

        if (RuleOrder == MarkThatPawnMod.instance.Settings.AutoRules.Where(rule => rule.IsOverride == IsOverride)
                .Max(rule => rule.RuleOrder))
        {
            return;
        }

        var ruleToSwitchWith = MarkThatPawnMod.instance.Settings.AutoRules.Where(rule => rule.IsOverride == IsOverride)
            .OrderBy(rule => rule.RuleOrder)
            .First(rule => rule.RuleOrder > RuleOrder);

        (ruleToSwitchWith.RuleOrder, RuleOrder) = (RuleOrder, ruleToSwitchWith.RuleOrder);
    }

    public void ShowPawnLimitationSelectorMenu()
    {
        if (ConfigError)
        {
            return;
        }

        var pawnTypeList = new List<FloatMenuOption>();

        foreach (var typeOfPawn in ApplicablePawnTypes)
        {
            switch (typeOfPawn)
            {
                case PawnType.Slave when !ModLister.IdeologyInstalled:
                case PawnType.Vehicle when !VehiclesLoaded:
                    continue;
                default:
                    pawnTypeList.Add(new FloatMenuOption($"MTP.PawnType.{typeOfPawn}".Translate(),
                        () => { PawnLimitation = typeOfPawn; }));
                    break;
            }
        }

        Find.WindowStack.Add(new FloatMenu(pawnTypeList));
    }
}