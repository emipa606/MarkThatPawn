using System.Collections.Generic;
using MarkThatPawn.MarkerRules;
using Verse;

namespace MarkThatPawn;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class MarkThatPawnSettings : ModSettings
{
    public List<string> AutoRuleBlobs = [];
    public List<MarkerRule> AutoRules = [];
    public bool ColonistDiffer;
    private string colonistMarkerSet = "WowStyle";
    private MarkerDef colonistMarkerSetDef;
    private string defaultMarkerSet = "WowStyle";
    private MarkerDef defaultMarkerSetDef;
    public bool EnemyDiffer;
    private string enemyMarkerSet = "WowStyle";
    private MarkerDef enemyMarkerSetDef;

    public float IconScalingFactor = 1f;
    public float IconSize = 0.7f;
    public bool NeutralDiffer;
    private string neutralMarkerSet = "WowStyle";
    private MarkerDef neutralMarkerSetDef;
    public bool PrisonerDiffer;
    private string prisonerMarkerSet = "WowStyle";
    private MarkerDef prisonerMarkerSetDef;
    public bool PulsatingIcons;
    public bool RelativeIconSize;
    public bool RelativeToZoom = true;
    public bool ShowForColonist = true;

    public bool ShowForEnemy = true;

    public bool ShowForNeutral = true;
    public bool ShowForPrisoner = true;

    public bool ShowForSlave = true;

    public bool ShowForVehicles = true;
    public bool SlaveDiffer;
    private string slaveMarkerSet = "WowStyle";
    private MarkerDef slaveMarkerSetDef;
    public bool VehiclesDiffer;
    private string vehiclesMarkerSet = "WowStyle";
    private MarkerDef vehiclesMarkerSetDef;
    public float XOffset;
    public float ZOffset;

    public MarkerDef DefaultMarkerSet
    {
        get => genericSetGetter(ref defaultMarkerSetDef, ref defaultMarkerSet);
        set
        {
            defaultMarkerSetDef = value;
            defaultMarkerSet = value.defName;
        }
    }

    public MarkerDef ColonistMarkerSet
    {
        get => genericSetGetter(ref colonistMarkerSetDef, ref colonistMarkerSet);
        set
        {
            colonistMarkerSetDef = value;
            colonistMarkerSet = value.defName;
        }
    }

    public MarkerDef PrisonerMarkerSet
    {
        get => genericSetGetter(ref prisonerMarkerSetDef, ref prisonerMarkerSet);
        set
        {
            prisonerMarkerSetDef = value;
            prisonerMarkerSet = value.defName;
        }
    }

    public MarkerDef SlaveMarkerSet
    {
        get => genericSetGetter(ref slaveMarkerSetDef, ref slaveMarkerSet);
        set
        {
            slaveMarkerSetDef = value;
            slaveMarkerSet = value.defName;
        }
    }

    public MarkerDef EnemyMarkerSet
    {
        get => genericSetGetter(ref enemyMarkerSetDef, ref enemyMarkerSet);
        set
        {
            enemyMarkerSetDef = value;
            enemyMarkerSet = value.defName;
        }
    }

    public MarkerDef NeutralMarkerSet
    {
        get => genericSetGetter(ref neutralMarkerSetDef, ref neutralMarkerSet);
        set
        {
            neutralMarkerSetDef = value;
            neutralMarkerSet = value.defName;
        }
    }

    public MarkerDef VehiclesMarkerSet
    {
        get => genericSetGetter(ref vehiclesMarkerSetDef, ref vehiclesMarkerSet);
        set
        {
            vehiclesMarkerSetDef = value;
            vehiclesMarkerSet = value.defName;
        }
    }


    private MarkerDef genericSetGetter(ref MarkerDef privateMarkerSetDef, ref string privateMarkerSet)
    {
        if (privateMarkerSetDef != null)
        {
            return privateMarkerSetDef;
        }

        privateMarkerSetDef = DefDatabase<MarkerDef>.GetNamedSilentFail(privateMarkerSet);
        if (privateMarkerSetDef != null)
        {
            return privateMarkerSetDef;
        }

        privateMarkerSet = "WowStyle";
        privateMarkerSetDef = DefDatabase<MarkerDef>.GetNamedSilentFail(privateMarkerSet);

        return privateMarkerSetDef;
    }

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref defaultMarkerSet, "defaultMarkerSet", "WowStyle");
        Scribe_Values.Look(ref colonistMarkerSet, "colonistMarkerSet", "WowStyle");
        Scribe_Values.Look(ref prisonerMarkerSet, "prisonerMarkerSet", "WowStyle");
        Scribe_Values.Look(ref slaveMarkerSet, "slaveMarkerSet", "WowStyle");
        Scribe_Values.Look(ref enemyMarkerSet, "enemyMarkerSet", "WowStyle");
        Scribe_Values.Look(ref neutralMarkerSet, "neutralMarkerSet", "WowStyle");
        Scribe_Values.Look(ref vehiclesMarkerSet, "vehiclesMarkerSet", "WowStyle");
        Scribe_Values.Look(ref PulsatingIcons, "PulsatingIcons");
        Scribe_Values.Look(ref RelativeToZoom, "RelativeToZoom", true);
        Scribe_Values.Look(ref IconScalingFactor, "IconScalingFactor", 1f);
        Scribe_Values.Look(ref ShowForColonist, "ShowForColonist", true);
        Scribe_Values.Look(ref ShowForPrisoner, "ShowForPrisoner", true);
        Scribe_Values.Look(ref ShowForSlave, "ShowForSlave", true);
        Scribe_Values.Look(ref ShowForEnemy, "ShowForEnemy", true);
        Scribe_Values.Look(ref ShowForNeutral, "ShowForNeutral", true);
        Scribe_Values.Look(ref ShowForVehicles, "ShowForVehicles", true);
        Scribe_Values.Look(ref ColonistDiffer, "ColonistDiffer");
        Scribe_Values.Look(ref PrisonerDiffer, "PrisonerDiffer");
        Scribe_Values.Look(ref RelativeIconSize, "RelativeIconSize");
        Scribe_Values.Look(ref SlaveDiffer, "SlaveDiffer");
        Scribe_Values.Look(ref EnemyDiffer, "EnemyDiffer");
        Scribe_Values.Look(ref NeutralDiffer, "NeutralDiffer");
        Scribe_Collections.Look(ref AutoRuleBlobs, "AutoRuleBlobs");
        Scribe_Values.Look(ref VehiclesDiffer, "VehiclesDiffer");
        Scribe_Values.Look(ref IconSize, "IconSize", 0.7f);
        Scribe_Values.Look(ref XOffset, "XOffset");
        Scribe_Values.Look(ref ZOffset, "ZOffset");
    }

    public void Reset()
    {
        DefaultMarkerSet = DefDatabase<MarkerDef>.GetNamedSilentFail("WowStyle");
        ColonistMarkerSet = DefDatabase<MarkerDef>.GetNamedSilentFail("WowStyle");
        PrisonerMarkerSet = DefDatabase<MarkerDef>.GetNamedSilentFail("WowStyle");
        SlaveMarkerSet = DefDatabase<MarkerDef>.GetNamedSilentFail("WowStyle");
        EnemyMarkerSet = DefDatabase<MarkerDef>.GetNamedSilentFail("WowStyle");
        NeutralMarkerSet = DefDatabase<MarkerDef>.GetNamedSilentFail("WowStyle");
        VehiclesMarkerSet = DefDatabase<MarkerDef>.GetNamedSilentFail("WowStyle");
        RelativeIconSize = false;
        RelativeToZoom = true;
        IconScalingFactor = 1f;
        ShowForColonist = true;
        ShowForPrisoner = true;
        ShowForSlave = true;
        ShowForEnemy = true;
        ShowForNeutral = true;
        ShowForVehicles = true;
        PulsatingIcons = false;
        ColonistDiffer = false;
        PrisonerDiffer = false;
        SlaveDiffer = false;
        EnemyDiffer = false;
        NeutralDiffer = false;
        VehiclesDiffer = false;
        IconSize = 0.7f;
        XOffset = 0f;
        ZOffset = 0f;
    }
}