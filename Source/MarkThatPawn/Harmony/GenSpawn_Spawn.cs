using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.Spawn), typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4),
    typeof(WipeMode), typeof(bool))]
public static class GenSpawn_Spawn
{
    public static void Postfix(Thing __result, Map map, bool respawningAfterLoad)
    {
        if (respawningAfterLoad)
        {
            return;
        }

        if (__result is not Pawn pawn)
        {
            return;
        }

        var tracker = map.GetComponent<MarkingTracker>();
        if (tracker?.PawnsToEvaluate.Contains(pawn) == true)
        {
            return;
        }

        tracker?.PawnsToEvaluate.Add(pawn);
    }
}