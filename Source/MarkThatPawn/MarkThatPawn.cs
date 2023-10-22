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
        Neutral
    }

    public static readonly Texture2D MarkerIcon;
    public static readonly Texture2D CancelIcon;
    public static readonly List<Mesh> SizeMesh;
    private static readonly List<MarkerDef> markerDefs;
    private static readonly Dictionary<Pawn, MarkerDef> pawnMarkerCache;

    static MarkThatPawn()
    {
        new Harmony("Mlie.MarkThatPawn").PatchAll(Assembly.GetExecutingAssembly());

        markerDefs = DefDatabase<MarkerDef>.AllDefsListForReading.OrderBy(def => def.label).ToList();
        pawnMarkerCache = new Dictionary<Pawn, MarkerDef>();

        MarkerIcon = ContentFinder<Texture2D>.Get("UI/Marker_Icon");
        CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        SizeMesh = new List<Mesh>();
        for (var i = 1; i <= 25; i++)
        {
            SizeMesh.Add(MeshMakerPlanes.NewPlaneMesh(i / 10f));
        }
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

        var drawPos = pawn.DrawPos;
        drawPos.x += MarkThatPawnMod.instance.Settings.XOffset;
        drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.28125f;
        drawPos.z += pawn.def.size.z + (MarkThatPawnMod.instance.Settings.IconSize / 3);
        drawPos.z += MarkThatPawnMod.instance.Settings.ZOffset;
        renderMarker(pawn, markerSet.MarkerMaterials[marker - 1], drawPos, getRightSizeMesh());
    }

    private static Mesh getRightSizeMesh()
    {
        var iconInt = (int)Math.Round(MarkThatPawnMod.instance.Settings.IconSize * 10);
        return SizeMesh.Count < iconInt ? MeshPool.plane10 : SizeMesh[iconInt - 1];
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
        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval) &&
            pawnMarkerCache.TryGetValue(pawn, out var markerDefForPawn))
        {
            return markerDefForPawn;
        }

        var markerSet = MarkThatPawnMod.instance.Settings.DefaultMarkerSet;

        if (pawn.IsColonist || pawn.IsColonyMech)
        {
            if (MarkThatPawnMod.instance.Settings.ColonistDiffer)
            {
                markerSet = MarkThatPawnMod.instance.Settings.ColonistMarkerSet;
            }

            pawnMarkerCache[pawn] = markerSet;
            return markerSet;
        }

        if (pawn.IsPrisonerOfColony)
        {
            if (MarkThatPawnMod.instance.Settings.PrisonerDiffer)
            {
                markerSet = MarkThatPawnMod.instance.Settings.PrisonerMarkerSet;
            }

            pawnMarkerCache[pawn] = markerSet;
            return markerSet;
        }

        if (pawn.IsSlaveOfColony)
        {
            if (MarkThatPawnMod.instance.Settings.SlaveDiffer)
            {
                markerSet = MarkThatPawnMod.instance.Settings.SlaveMarkerSet;
            }

            pawnMarkerCache[pawn] = markerSet;
            return markerSet;
        }

        if (pawn.HostileTo(Faction.OfPlayer))
        {
            if (MarkThatPawnMod.instance.Settings.EnemyDiffer)
            {
                markerSet = MarkThatPawnMod.instance.Settings.EnemyMarkerSet;
            }

            pawnMarkerCache[pawn] = markerSet;
            return markerSet;
        }

        if (MarkThatPawnMod.instance.Settings.NeutralDiffer)
        {
            markerSet = MarkThatPawnMod.instance.Settings.NeutralMarkerSet;
        }

        pawnMarkerCache[pawn] = markerSet;
        return markerSet;
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
            }


            returnList.Add(new FloatMenuOption(markingSet.LabelCap, action, markingSet.Icon, Color.white));
        }

        return returnList;
    }
}