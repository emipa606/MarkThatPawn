using System.Collections.Generic;
using CombatAI;
using Verse;

namespace MarkThatPawn;

public static class CAI5000FogCheck
{
    private static readonly Dictionary<Map, MapComponent> fogGrids = new();

    public static bool IsFogged(Pawn pawn)
    {
        var map = pawn?.Map;
        if (map == null)
        {
            return false;
        }

        var cell = pawn.Position;
        return cell != IntVec3.Invalid && ((MapComponent_FogGrid)getFogGrid(map)).IsFogged(cell);
    }

    private static MapComponent getFogGrid(Map map)
    {
        if (fogGrids.TryGetValue(map, out var fogGrid))
        {
            return fogGrid;
        }

        fogGrid = map.GetComponent<MapComponent_FogGrid>();
        fogGrids[map] = fogGrid;

        return fogGrid;
    }
}