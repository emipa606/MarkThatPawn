using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

[StaticConstructorOnStartup]
public static class MarkThatPawn
{
    public enum PawnType
    {
        Default,
        Colonist,
        Prisoner,
        Slave,
        Enemy,
        Neutral,
        Vehicle
    }

    public const float ButtonIconSizeFactor = 0.8f;
    public static readonly List<TraitDef> AllTraits;
    public static readonly List<SkillDef> AllSkills;
    public static readonly List<ThingDef> AllValidWeapons;
    public static readonly List<ThingDef> AllExplosiveRangedWeapons;
    public static readonly List<ThingDef> AllThrownWeapons;
    public static readonly Texture2D MarkerIcon;
    public static readonly Texture2D CancelIcon;
    public static readonly List<Mesh> SizeMesh;
    public static readonly List<MarkerDef> MarkerDefs;
    private static readonly Dictionary<Pawn, MarkerDef> pawnMarkerCache;
    private static readonly Dictionary<Pawn, Mesh> pawnMeshCache;
    public static readonly bool VehiclesLoaded;
    private static CameraZoomRange lastCameraZoomRange = CameraZoomRange.Far;
    private static readonly int standardSize;
    private static readonly Texture2D autoIcon;

    static MarkThatPawn()
    {
        var harmony = new Harmony("Mlie.MarkThatPawn");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        AllTraits = DefDatabase<TraitDef>.AllDefsListForReading.OrderBy(def => def.label).ToList();
        AllSkills = DefDatabase<SkillDef>.AllDefsListForReading.OrderBy(def => def.label).ToList();
        MarkerDefs = DefDatabase<MarkerDef>.AllDefsListForReading.Where(def => def.Enabled).OrderBy(def => def.label)
            .ToList();
        pawnMarkerCache = new Dictionary<Pawn, MarkerDef>();
        pawnMeshCache = new Dictionary<Pawn, Mesh>();
        standardSize = ThingDefOf.Human.size.z;
        MarkerIcon = ContentFinder<Texture2D>.Get("UI/Marker_Icon");
        CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        autoIcon = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/Scheduled");
        SizeMesh = new List<Mesh>();
        for (var i = 1; i <= 50; i++)
        {
            SizeMesh.Add(MeshMakerPlanes.NewPlaneMesh(i / 10f));
        }

        AllValidWeapons = DefDatabase<ThingDef>.AllDefsListForReading
            .Where(def => !string.IsNullOrEmpty(def.label) && def.IsWeapon)
            .OrderBy(def => def.label).ToList();

        AllExplosiveRangedWeapons = AllValidWeapons
            .Where(def => def.IsRangedWeapon && def.Verbs.Any(properties =>
                properties.CausesExplosion && properties.defaultProjectile?.projectile.arcHeightFactor == 0)).ToList();
        AllThrownWeapons = AllValidWeapons.Where(def =>
            def.Verbs.Any(properties => properties.defaultProjectile?.projectile.arcHeightFactor > 0)).ToList();

        Log.Message(
            $"[MarkThatPawn]: Found {AllValidWeapons.Count} loaded weapons, {AllExplosiveRangedWeapons.Count} explosive weapons and {AllThrownWeapons.Count} thrown projectiles");

        foreach (var ruleBlob in MarkThatPawnMod.instance.Settings.AutoRuleBlobs)
        {
            MarkerRule rule;
            if (!MarkerRule.TryGetRuleTypeFromBlob(ruleBlob, out var type))
            {
                continue;
            }

            switch (type)
            {
                case MarkerRule.AutoRuleType.Weapon:
                    rule = new WeaponMarkerRule(ruleBlob);
                    break;
                case MarkerRule.AutoRuleType.WeaponType:
                    rule = new WeaponTypeMarkerRule(ruleBlob);
                    break;
                case MarkerRule.AutoRuleType.Trait:
                    rule = new TraitMarkerRule(ruleBlob);
                    break;
                case MarkerRule.AutoRuleType.Skill:
                    rule = new SkillMarkerRule(ruleBlob);
                    break;
                case MarkerRule.AutoRuleType.Relative:
                    rule = new RelativeMarkerRule(ruleBlob);
                    break;
                case MarkerRule.AutoRuleType.PawnType:
                    rule = new PawnTypeMarkerRule(ruleBlob);
                    break;
                case MarkerRule.AutoRuleType.Drafted:
                    rule = new DraftedMarkerRule(ruleBlob);
                    break;
                case MarkerRule.AutoRuleType.MentalState:
                    rule = new MentalStateMarkerRule(ruleBlob);
                    break;
                default:
                    continue;
            }

            if (rule.ConfigError)
            {
                Log.Warning(
                    $"Failed to load a marker-rule from blob: \n{ruleBlob}\n{rule.ErrorMessage}\nDisabling the rule.");
                rule.SetEnabled(false);
            }

            MarkThatPawnMod.instance.Settings.AutoRules.Add(rule);
        }

        Log.Message(
            $"[MarkThatPawn]: Found {MarkThatPawnMod.instance.Settings.AutoRules.Count} automatic rules defined");

        VehiclesLoaded = ModLister.GetActiveModWithIdentifier("SmashPhil.VehicleFramework") != null;
        if (!VehiclesLoaded)
        {
            return;
        }

        Log.Message("[MarkThatPawn]: Vehicle Framework detected, adding compatility patch");
        var original = AccessTools.Method("Vehicles.VehicleRenderer:RenderPawnAt");
        var postfix = typeof(VehicleRenderer_RenderPawnAt).GetMethod(nameof(VehicleRenderer_RenderPawnAt.Postfix));
        harmony.Patch(original, postfix: new HarmonyMethod(postfix));
    }


    public static void RenderMarkingOverlay(Pawn pawn, int marker, MarkingTracker tracker)
    {
        if (!pawn.Spawned)
        {
            return;
        }

        Material material;

        switch (marker)
        {
            case > 0:
                var markerSet = GetMarkerDefForPawn(pawn);
                if (markerSet == null)
                {
                    return;
                }

                if (markerSet.MarkerMaterials.Count < marker)
                {
                    return;
                }

                material = markerSet.MarkerMaterials[marker - 1];
                break;
            case -1:
                if (!tracker.GlobalMarkingTracker.AutomaticPawns.TryGetValue(pawn, out var autoString))
                {
                    return;
                }

                if (!TryToConvertStringToMaterial(autoString, out material))
                {
                    return;
                }

                break;
            case -2:
                if (!tracker.GlobalMarkingTracker.CustomPawns.TryGetValue(pawn, out var customString))
                {
                    return;
                }

                if (!TryToConvertStringToMaterial(customString, out material))
                {
                    return;
                }

                break;
            case -3:
                if (!tracker.GlobalMarkingTracker.OverridePawns.TryGetValue(pawn, out var overrideString))
                {
                    return;
                }

                var overrideStringTypeless = overrideString.Split('§')[0];

                if (!TryToConvertStringToMaterial(overrideStringTypeless, out material))
                {
                    return;
                }

                break;
            default:
                return;
        }

        var pawnHeight = pawn.def.size.z;
        if (VehiclesLoaded && pawn.def.thingClass.Name.EndsWith("VehiclePawn"))
        {
            pawnHeight = standardSize + (int)Math.Floor((pawn.def.size.z - standardSize) / 2f);
        }

        var drawPos = pawn.DrawPos;
        drawPos.x += MarkThatPawnMod.instance.Settings.XOffset;
        drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.28125f;
        drawPos.z += pawnHeight + (MarkThatPawnMod.instance.Settings.IconSize / 3);
        drawPos.z += MarkThatPawnMod.instance.Settings.ZOffset;
        renderMarker(pawn, material, drawPos, getRightSizeMesh(pawn));
    }

    private static Mesh getRightSizeMesh(Pawn pawn)
    {
        if (MarkThatPawnMod.instance.Settings.RelativeToZoom && lastCameraZoomRange != Find.CameraDriver.CurrentZoom)
        {
            pawnMeshCache.Clear();
            lastCameraZoomRange = Find.CameraDriver.CurrentZoom;
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval) &&
            pawnMeshCache.TryGetValue(pawn, out var meshForPawn))
        {
            return meshForPawn;
        }

        var iconInt = (int)Math.Round(MarkThatPawnMod.instance.Settings.IconSize * 10);

        if (MarkThatPawnMod.instance.Settings.RelativeIconSize)
        {
            var relativeSize = ((pawn.def.size.z - standardSize) / 2) + 1;
            iconInt = Math.Min((int)Math.Round(relativeSize * (float)iconInt), SizeMesh.Count);
        }

        if (MarkThatPawnMod.instance.Settings.RelativeToZoom)
        {
            iconInt = Math.Min(
                iconInt + (int)Math.Round((int)Find.CameraDriver.CurrentZoom *
                                          MarkThatPawnMod.instance.Settings.IconScalingFactor), SizeMesh.Count);
        }

        pawnMeshCache[pawn] = SizeMesh.Count < iconInt ? MeshPool.plane10 : SizeMesh[iconInt - 1];
        return pawnMeshCache[pawn];
    }

    private static void renderMarker(Pawn pawn, Material material, Vector3 drawPos, Mesh mesh)
    {
        if (!MarkThatPawnMod.instance.Settings.PulsatingIcons)
        {
            Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
            return;
        }

        var iterator = (Time.realtimeSinceStartup + (397f * (pawn.thingIDNumber % 571))) * 4f;
        var pulsatingCyclePlace = ((float)Math.Sin(iterator) + 1f) * 0.5f;
        pulsatingCyclePlace = 0.3f + (pulsatingCyclePlace * 0.7f);

        var fadedMaterial = FadedMaterialPool.FadedVersionOf(material, pulsatingCyclePlace);
        Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, fadedMaterial, 0);
    }


    public static bool TryGetAutoMarkerForPawn(Pawn pawn, out string result, Type specificType = null)
    {
        result = null;
        if (MarkThatPawnMod.instance.Settings.AutoRules == null || !MarkThatPawnMod.instance.Settings.AutoRules.Any())
        {
            return false;
        }

        if (!ValidPawn(pawn))
        {
            return false;
        }

        foreach (var markerRule in MarkThatPawnMod.instance.Settings.AutoRules.Where(rule => rule.Enabled)
                     .OrderBy(rule => rule.RuleOrder))
        {
            if (specificType != null && markerRule.GetType() != specificType)
            {
                continue;
            }

            if (!markerRule.AppliesToPawn(pawn))
            {
                continue;
            }

            result = markerRule.GetMarkerBlob();
            return true;
        }

        return false;
    }


    public static bool TryToConvertStringToMaterial(string markerString, out Material result)
    {
        result = null;
        if (markerString == null || !markerString.Contains(";"))
        {
            return false;
        }

        var markerSet = MarkerDefs.FirstOrDefault(def => def.defName == markerString.Split(';')[0]);
        if (markerSet == null)
        {
            return false;
        }

        if (!int.TryParse(markerString.Split(';')[1], out var number))
        {
            return false;
        }

        if (markerSet.MarkerMaterials.Count < number)
        {
            return false;
        }

        result = markerSet.MarkerMaterials[number - 1];
        return true;
    }

    public static bool TryToConvertStringToTexture2D(string markerString, out Texture2D result)
    {
        result = null;
        if (markerString == null || !markerString.Contains(";"))
        {
            return false;
        }

        var markerSet = MarkerDefs.FirstOrDefault(def => def.defName == markerString.Split(';')[0]);
        if (markerSet == null)
        {
            return false;
        }

        if (!int.TryParse(markerString.Split(';')[1], out var number))
        {
            return false;
        }

        if (markerSet.MarkerTextures.Count < number)
        {
            return false;
        }

        result = markerSet.MarkerTextures[number - 1];
        return true;
    }


    public static MarkerDef GetMarkerDefForPawn(Pawn pawn)
    {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval) &&
            pawnMarkerCache.TryGetValue(pawn, out var markerDefForPawn))
        {
            return markerDefForPawn;
        }

        switch (pawn.GetPawnType())
        {
            case PawnType.Colonist when MarkThatPawnMod.instance.Settings.ColonistDiffer:
                pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.ColonistMarkerSet;
                break;
            case PawnType.Prisoner when MarkThatPawnMod.instance.Settings.PrisonerDiffer:
                pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.PrisonerMarkerSet;
                break;
            case PawnType.Slave when MarkThatPawnMod.instance.Settings.SlaveDiffer:
                pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.SlaveMarkerSet;
                break;
            case PawnType.Enemy when MarkThatPawnMod.instance.Settings.EnemyDiffer:
                pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.EnemyMarkerSet;
                break;
            case PawnType.Neutral when MarkThatPawnMod.instance.Settings.NeutralDiffer:
                pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.NeutralMarkerSet;
                break;
            case PawnType.Vehicle when MarkThatPawnMod.instance.Settings.VehiclesDiffer:
                pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.VehiclesMarkerSet;
                break;
            default:
                pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.DefaultMarkerSet;
                break;
        }

        return pawnMarkerCache[pawn];
    }

    public static void ResetCache(Pawn pawn = null)
    {
        if (pawn == null)
        {
            pawnMeshCache.Clear();
            pawnMarkerCache.Clear();
            return;
        }

        if (pawnMeshCache.ContainsKey(pawn))
        {
            pawnMeshCache.Remove(pawn);
        }

        if (pawnMarkerCache.ContainsKey(pawn))
        {
            pawnMarkerCache.Remove(pawn);
        }
    }

    public static PawnType GetPawnType(this Pawn pawn)
    {
        if (VehiclesLoaded && pawn.def.thingClass.Name.EndsWith("VehiclePawn"))
        {
            return PawnType.Vehicle;
        }

        if (ModLister.RoyaltyInstalled && pawn.IsSlaveOfColony)
        {
            return PawnType.Slave;
        }

        if (pawn.IsColonist)
        {
            return PawnType.Colonist;
        }

        if (pawn.IsPrisonerOfColony)
        {
            return PawnType.Prisoner;
        }

        return pawn.HostileTo(Faction.OfPlayer) ? PawnType.Enemy : PawnType.Neutral;
    }

    public static bool ValidPawn(Pawn pawn)
    {
        if (pawn == null)
        {
            return false;
        }

        if (!pawn.Spawned)
        {
            return false;
        }

        if (pawn.Map == null)
        {
            return false;
        }

        switch (pawn.GetPawnType())
        {
            case PawnType.Colonist:
                return MarkThatPawnMod.instance.Settings.ShowForColonist;
            case PawnType.Prisoner:
                return MarkThatPawnMod.instance.Settings.ShowForPrisoner;
            case PawnType.Slave:
                return MarkThatPawnMod.instance.Settings.ShowForSlave;
            case PawnType.Enemy:
                return MarkThatPawnMod.instance.Settings.ShowForEnemy;
            case PawnType.Neutral:
                return MarkThatPawnMod.instance.Settings.ShowForNeutral;
            case PawnType.Vehicle:
                return MarkThatPawnMod.instance.Settings.ShowForVehicles;
            default:
                return true;
        }
    }

    public static bool TryGetMarkerDef(string markerDefName, out MarkerDef result)
    {
        result = null;
        if (MarkerDefs == null || !MarkerDefs.Any())
        {
            return false;
        }

        result = MarkerDefs.FirstOrDefault(def => def.defName == markerDefName);
        return result != null;
    }

    public static List<FloatMenuOption> GetMarkingOptions(int currentMarking, MarkingTracker tracker,
        MarkerDef markerSet, Pawn pawn)
    {
        var returnList = new List<FloatMenuOption>();
        if (tracker.GlobalMarkingTracker.AutomaticPawns == null)
        {
            tracker.GlobalMarkingTracker.AutomaticPawns = new Dictionary<Pawn, string>();
        }

        if (tracker.GlobalMarkingTracker.CustomPawns == null)
        {
            tracker.GlobalMarkingTracker.CustomPawns = new Dictionary<Pawn, string>();
        }

        void CustomAction()
        {
            var markerMenu = new List<FloatMenuOption>();
            foreach (var markerDef in MarkerDefs)
            {
                markerMenu.Add(new FloatMenuOption(markerDef.LabelCap, () =>
                {
                    var markerNumber = new List<FloatMenuOption>();
                    for (var i = 0; i < markerDef.MarkerTextures.Count; i++)
                    {
                        var mark = i + 1;

                        void Action()
                        {
                            tracker.GlobalMarkingTracker.SetPawnMarking(pawn, -2, currentMarking,
                                customMarkerString: $"{markerDef.defName};{mark}");
                        }

                        markerNumber.Add(new FloatMenuOption("MTP.MarkerNumber".Translate(mark), Action,
                            markerDef.MarkerTextures[i], Color.white));
                    }

                    Find.WindowStack.Add(new FloatMenu(markerNumber));
                }, markerDef.Icon, Color.white));
            }

            Find.WindowStack.Add(new FloatMenu(markerMenu));
        }

        returnList.Add(new FloatMenuOption("MTP.CustomIcon".Translate(), CustomAction, TexButton.NewItem,
            Color.white));

        if (tracker.GlobalMarkingTracker.AutomaticPawns.TryGetValue(pawn, out _))
        {
            void AutoAction()
            {
                tracker.GlobalMarkingTracker.SetPawnMarking(pawn, -1, currentMarking);
            }

            returnList.Add(new FloatMenuOption("MTP.UseAutoIcon".Translate(), AutoAction, autoIcon, Color.white));
        }

        for (var i = 0; i <= markerSet.MarkerTextures.Count; i++)
        {
            var mark = i;

            void Action()
            {
                tracker.GlobalMarkingTracker.SetPawnMarking(pawn, mark, currentMarking);
            }

            Texture2D icon;
            TaggedString title;
            if (i == 0)
            {
                title = "MTP.None".Translate(i);
                icon = CancelIcon;
            }
            else
            {
                title = "MTP.MarkerNumber".Translate(i);
                icon = markerSet.MarkerTextures[i - 1];
            }

            returnList.Add(new FloatMenuOption(title, Action, icon, Color.white));
        }

        return returnList;
    }

    public static List<FloatMenuOption> GetMarkingSetOptions(PawnType type)
    {
        var returnList = new List<FloatMenuOption>();
        foreach (var markingSet in MarkerDefs)
        {
            var action = () => { MarkThatPawnMod.instance.Settings.DefaultMarkerSet = markingSet; };

            switch (type)
            {
                case PawnType.Colonist:
                    action = () => { MarkThatPawnMod.instance.Settings.ColonistMarkerSet = markingSet; };
                    break;
                case PawnType.Prisoner:
                    action = () => { MarkThatPawnMod.instance.Settings.PrisonerMarkerSet = markingSet; };
                    break;
                case PawnType.Slave:
                    action = () => { MarkThatPawnMod.instance.Settings.SlaveMarkerSet = markingSet; };
                    break;
                case PawnType.Enemy:
                    action = () => { MarkThatPawnMod.instance.Settings.EnemyMarkerSet = markingSet; };
                    break;
                case PawnType.Neutral:
                    action = () => { MarkThatPawnMod.instance.Settings.NeutralMarkerSet = markingSet; };
                    break;
                case PawnType.Vehicle:
                    action = () => { MarkThatPawnMod.instance.Settings.VehiclesMarkerSet = markingSet; };
                    break;
            }


            returnList.Add(new FloatMenuOption(markingSet.LabelCap, action, markingSet.Icon, Color.white));
        }

        return returnList;
    }
}