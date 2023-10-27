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
    public enum PawnMarkingType
    {
        Default,
        Colonist,
        Prisoner,
        Slave,
        Enemy,
        Neutral,
        Vehicle
    }

    public static readonly Texture2D MarkerIcon;
    public static readonly Texture2D CancelIcon;
    public static readonly List<Mesh> SizeMesh;
    private static readonly List<MarkerDef> markerDefs;
    private static readonly Dictionary<Pawn, MarkerDef> pawnMarkerCache;
    private static readonly Dictionary<Pawn, Mesh> pawnMeshCache;
    public static readonly bool VehiclesLoaded;
    private static CameraZoomRange lastCameraZoomRange = CameraZoomRange.Far;
    private static readonly int standardSize;

    static MarkThatPawn()
    {
        var harmony = new Harmony("Mlie.MarkThatPawn");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        markerDefs = DefDatabase<MarkerDef>.AllDefsListForReading.OrderBy(def => def.label).ToList();
        pawnMarkerCache = new Dictionary<Pawn, MarkerDef>();
        pawnMeshCache = new Dictionary<Pawn, Mesh>();
        standardSize = ThingDefOf.Human.size.z;
        MarkerIcon = ContentFinder<Texture2D>.Get("UI/Marker_Icon");
        CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        SizeMesh = new List<Mesh>();
        for (var i = 1; i <= 50; i++)
        {
            SizeMesh.Add(MeshMakerPlanes.NewPlaneMesh(i / 10f));
        }

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

    public static void RenderMarkingOverlay(Pawn pawn, int marker)
    {
        if (!pawn.Spawned)
        {
            return;
        }

        var markerSet = GetMarkerDefForPawn(pawn);
        if (markerSet == null)
        {
            return;
        }

        if (markerSet.MarkerMaterials.Count < marker)
        {
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
        renderMarker(pawn, markerSet.MarkerMaterials[marker - 1], drawPos, getRightSizeMesh(pawn));
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


    public static MarkerDef GetMarkerDefForPawn(Pawn pawn)
    {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval) &&
            pawnMarkerCache.TryGetValue(pawn, out var markerDefForPawn))
        {
            return markerDefForPawn;
        }

        if (VehiclesLoaded && MarkThatPawnMod.instance.Settings.VehiclesDiffer &&
            pawn.def.thingClass.Name.EndsWith("VehiclePawn"))
        {
            pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.VehiclesMarkerSet;
            return pawnMarkerCache[pawn];
        }

        if (MarkThatPawnMod.instance.Settings.ColonistDiffer && (pawn.IsColonist || pawn.IsColonyMech))
        {
            pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.ColonistMarkerSet;
            return pawnMarkerCache[pawn];
        }

        if (MarkThatPawnMod.instance.Settings.PrisonerDiffer && pawn.IsPrisonerOfColony)
        {
            pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.PrisonerMarkerSet;
            return pawnMarkerCache[pawn];
        }

        if (MarkThatPawnMod.instance.Settings.SlaveDiffer && pawn.IsSlaveOfColony)
        {
            pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.SlaveMarkerSet;
            return pawnMarkerCache[pawn];
        }

        if (MarkThatPawnMod.instance.Settings.EnemyDiffer && pawn.HostileTo(Faction.OfPlayer))
        {
            pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.EnemyMarkerSet;
            return pawnMarkerCache[pawn];
        }

        if (MarkThatPawnMod.instance.Settings.NeutralDiffer)
        {
            pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.NeutralMarkerSet;
            return pawnMarkerCache[pawn];
        }

        pawnMarkerCache[pawn] = MarkThatPawnMod.instance.Settings.DefaultMarkerSet;
        return pawnMarkerCache[pawn];
    }

    public static void ResetCache()
    {
        pawnMeshCache.Clear();
        pawnMarkerCache.Clear();
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

        if (!MarkThatPawnMod.instance.Settings.ShowForColonist && pawn.IsColonist)
        {
            return false;
        }

        if (!MarkThatPawnMod.instance.Settings.ShowForPrisoner && pawn.IsPrisoner)
        {
            return false;
        }

        if (!MarkThatPawnMod.instance.Settings.ShowForSlave && pawn.IsSlave)
        {
            return false;
        }

        if (!MarkThatPawnMod.instance.Settings.ShowForEnemy && pawn.HostileTo(Faction.OfPlayer))
        {
            return false;
        }

        if (!MarkThatPawnMod.instance.Settings.ShowForNeutral && !pawn.HostileTo(Faction.OfPlayer))
        {
            return false;
        }

        return MarkThatPawnMod.instance.Settings.ShowForVehicles || !pawn.def.thingClass.Name.EndsWith("VehiclePawn");
    }

    public static List<FloatMenuOption> GetMarkingOptions(int currentMarking, MarkingTracker tracker,
        MarkerDef markerSet, Pawn pawn)
    {
        var returnList = new List<FloatMenuOption>();
        for (var i = 0; i <= markerSet.MarkerTextures.Count; i++)
        {
            var mark = i;

            void Action()
            {
                tracker.SetPawnMarking(pawn, mark, currentMarking, tracker);
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

    public static List<FloatMenuOption> GetMarkingSetOptions(PawnMarkingType type)
    {
        var returnList = new List<FloatMenuOption>();
        foreach (var markingSet in markerDefs)
        {
            var action = () => { MarkThatPawnMod.instance.Settings.DefaultMarkerSet = markingSet; };

            switch (type)
            {
                case PawnMarkingType.Colonist:
                    action = () => { MarkThatPawnMod.instance.Settings.ColonistMarkerSet = markingSet; };
                    break;
                case PawnMarkingType.Prisoner:
                    action = () => { MarkThatPawnMod.instance.Settings.PrisonerMarkerSet = markingSet; };
                    break;
                case PawnMarkingType.Slave:
                    action = () => { MarkThatPawnMod.instance.Settings.SlaveMarkerSet = markingSet; };
                    break;
                case PawnMarkingType.Enemy:
                    action = () => { MarkThatPawnMod.instance.Settings.EnemyMarkerSet = markingSet; };
                    break;
                case PawnMarkingType.Neutral:
                    action = () => { MarkThatPawnMod.instance.Settings.NeutralMarkerSet = markingSet; };
                    break;
                case PawnMarkingType.Vehicle:
                    action = () => { MarkThatPawnMod.instance.Settings.VehiclesMarkerSet = markingSet; };
                    break;
            }


            returnList.Add(new FloatMenuOption(markingSet.LabelCap, action, markingSet.Icon, Color.white));
        }

        return returnList;
    }
}