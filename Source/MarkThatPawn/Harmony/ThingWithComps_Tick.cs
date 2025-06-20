using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(Pawn), "Tick")]
public static class Pawn_Tick
{
    public static void Postfix(Pawn __instance)
    {
        if (!MarkThatPawnMod.Instance.Settings.RefreshRules || __instance == null ||
            !__instance.IsHashIntervalTick(GenTicks.TickLongInterval))
        {
            return;
        }

        var markingTracker = __instance.Map?.GetComponent<MarkingTracker>();
        if (markingTracker?.PawnsToEvaluate != null && !markingTracker.PawnsToEvaluate.Contains(__instance))
        {
            markingTracker.PawnsToEvaluate.Add(__instance);
        }

        if (__instance.Faction != null)
        {
            MarkThatPawn.FactionMaterialCache.Remove(__instance.Faction);
        }

        if (__instance.Ideo != null)
        {
            MarkThatPawn.IdeoMaterialCache.Remove(__instance.Ideo);
        }
    }
}