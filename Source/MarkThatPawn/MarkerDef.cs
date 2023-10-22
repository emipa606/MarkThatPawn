using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

public class MarkerDef : Def
{
    private string graphicPrefix;
    private Texture2D icon;
    private string iconPath;
    private List<Material> markerMaterials;


    private List<Texture2D> markerTextures;

    public List<Material> MarkerMaterials
    {
        get
        {
            if (markerMaterials == null)
            {
                loadTextures();
            }

            return markerMaterials;
        }
    }

    public List<Texture2D> MarkerTextures
    {
        get
        {
            if (markerTextures == null)
            {
                loadTextures();
            }

            return markerTextures;
        }
    }

    public Texture2D Icon
    {
        get
        {
            if (icon == null)
            {
                loadTextures();
            }

            return icon;
        }
    }

    private void loadTextures()
    {
        icon = ContentFinder<Texture2D>.Get(iconPath, false);
        markerTextures = new List<Texture2D>();
        markerMaterials = new List<Material>();

        var counter = 0;
        for (var i = 0; i < 100; i++)
        {
            counter++;
            var foundTexture = ContentFinder<Texture2D>.Get($"{graphicPrefix}_{counter}", false);
            if (foundTexture == null)
            {
                break;
            }

            MarkerTextures.Add(foundTexture);
            MarkerMaterials.Add(MaterialPool.MatFrom($"{graphicPrefix}_{counter}", ShaderDatabase.MetaOverlay));
        }

        if (MarkerTextures.Any())
        {
            Log.Message($"[MarkThatPawn]: Found {MarkerTextures.Count} icons for {LabelCap}");
        }
    }
}