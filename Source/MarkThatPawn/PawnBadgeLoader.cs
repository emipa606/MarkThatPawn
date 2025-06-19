using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using RR_PawnBadge;
using Verse;

namespace MarkThatPawn;

public static class PawnBadgeLoader
{
    public static void LoadAllPawnBadges()
    {
        var badgeDefs = DefDatabase<BadgeDef>.AllDefs.ToList();
        var listOfBadgeFolders = new List<string>();

        Log.Message(
            $"[MarkThatPawn]: Pawn Badge mod loaded, trying to generate marker-sets of {badgeDefs.Count} badge definitions");

        foreach (var badgeDef in badgeDefs)
        {
            var iconBasePath = badgeDef.icon[..badgeDef.icon.LastIndexOf('/')];
            if (listOfBadgeFolders.Contains(iconBasePath))
            {
                continue;
            }

            listOfBadgeFolders.Add(iconBasePath);
        }

        foreach (var badgeFolder in listOfBadgeFolders)
        {
            var iconName = badgeFolder;
            if (badgeFolder.Contains("/"))
            {
                iconName = badgeFolder[(badgeFolder.LastIndexOf('/') + 1)..];
            }

            var badgeMarkerDef = new MarkerDef
            {
                defName = $"PawnBadge_{Regex.Replace(iconName, @"\s+", "")}",
                label = $"{iconName} - PawnBadge",
                description = $"{iconName} - Generated from PawnBadge definition",
                graphicPrefix = badgeFolder
            };
            badgeMarkerDef.LoadAllTexturesInFolder();
            DefGenerator.AddImpliedDef(badgeMarkerDef);
        }

        Log.Message(
            $"[MarkThatPawn]: Imported all pawn badges into {listOfBadgeFolders.Count} marker-sets");
    }
}