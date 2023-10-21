using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

[StaticConstructorOnStartup]
public static class MarkThatPawn
{
    private static readonly List<Texture2D> markerTextures;
    public static readonly Texture2D MarkerIcon;
    public static readonly Texture2D CancelIcon;
    public static readonly List<Mesh> SizeMesh;

    static MarkThatPawn()
    {
        new Harmony("Mlie.MarkThatPawn").PatchAll(Assembly.GetExecutingAssembly());

        markerTextures = new List<Texture2D>();
        MarkerIcon = ContentFinder<Texture2D>.Get("UI/Marker_Icon");
        CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        SizeMesh = new List<Mesh>();
        for (var i = 1; i <= 25; i++)
        {
            SizeMesh.Add(MeshMakerPlanes.NewPlaneMesh(i / 10f));
        }

        var counter = 0;
        for (var i = 0; i < 100; i++)
        {
            counter++;
            var foundTexture = ContentFinder<Texture2D>.Get($"UI/Overlays/Marker_{counter}", false);
            if (foundTexture == null)
            {
                break;
            }

            markerTextures.Add(foundTexture);
        }

        Log.Message($"[MarkThatPawn]: Found {markerTextures.Count} icons for marking pawns");
    }

    public static void RenderMarkingOverlay(Pawn pawn, int marker)
    {
        if (!pawn.Spawned)
        {
            return;
        }

        var drawPos = pawn.DrawPos;
        drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.28125f;
        drawPos.z += pawn.def.size.z + (MarkThatPawnMod.instance.Settings.IconSize / 3);


        renderPulsingMarker(pawn, marker, drawPos, getRightSizeMesh());
    }

    private static Mesh getRightSizeMesh()
    {
        var iconInt = (int)Math.Round(MarkThatPawnMod.instance.Settings.IconSize * 10);
        return SizeMesh.Count < iconInt ? MeshPool.plane10 : SizeMesh[iconInt - 1];
    }

    private static void renderPulsingMarker(Pawn pawn, int marker, Vector3 drawPos, Mesh mesh)
    {
        var markerMaterial = getMaterialForMarker(marker);
        if (markerMaterial == null)
        {
            return;
        }

        if (!MarkThatPawnMod.instance.Settings.PulsatingIcons)
        {
            Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, markerMaterial, 0);
            return;
        }

        var iterator = (Time.realtimeSinceStartup + (397f * (pawn.thingIDNumber % 571))) * 4f;
        var pulsatingCyclePlace = ((float)Math.Sin(iterator) + 1f) * 0.5f;
        pulsatingCyclePlace = 0.3f + (pulsatingCyclePlace * 0.7f);

        var material = FadedMaterialPool.FadedVersionOf(markerMaterial, pulsatingCyclePlace);
        Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
    }

    private static Material getMaterialForMarker(int marker)
    {
        if (marker == 0)
        {
            return null;
        }

        return marker > GetTotalAmountOfMarkers()
            ? null
            : MaterialPool.MatFrom($"UI/Overlays/Marker_{marker}", ShaderDatabase.MetaOverlay);
    }

    public static Texture2D GetTextureForMarker(int marker)
    {
        if (marker == 0)
        {
            return null;
        }

        return markerTextures.Count < marker ? null : markerTextures[marker - 1];
    }

    public static int GetTotalAmountOfMarkers()
    {
        return markerTextures.Count;
    }

    public static List<FloatMenuOption> GetMarkingOptions(int alreadySelected, MarkingTracker tracker, Pawn pawn)
    {
        var returnList = new List<FloatMenuOption>();

        for (var i = 0; i <= GetTotalAmountOfMarkers(); i++)
        {
            if (i == alreadySelected)
            {
                continue;
            }

            var mark = i;

            void Action()
            {
                tracker.SetPawnMarking(pawn, mark);
            }

            var title = "MTP.MarkerNumber".Translate(i);
            var icon = GetTextureForMarker(i);

            if (i == 0)
            {
                title = "MTP.None".Translate(i);
                icon = CancelIcon;
            }

            returnList.Add(new FloatMenuOption(title, Action, icon, Color.white));
        }

        return returnList;
    }
}