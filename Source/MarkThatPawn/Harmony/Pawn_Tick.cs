using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
public static class Pawn_Tick
{
    public static void Postfix(Pawn __instance)
    {
        if (!MarkThatPawnMod.instance.Settings.RefreshRules ||
            !__instance.IsHashIntervalTick(GenTicks.TickLongInterval))
        {
            return;
        }

        var markingTracker = __instance?.Map?.GetComponent<MarkingTracker>();
        if (markingTracker?.PawnsToEvaluate != null && !markingTracker.PawnsToEvaluate.Contains(__instance))
        {
            markingTracker.PawnsToEvaluate.Add(__instance);
        }
    }
}