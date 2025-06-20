using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(Corpse), nameof(Corpse.TickRare))]
public static class Corpse_TickRare
{
    public static void Postfix(Corpse __instance)
    {
        if (!MarkThatPawnMod.Instance.Settings.RefreshRules || !MarkThatPawnMod.Instance.Settings.ShowOnCorpses ||
            __instance == null)
        {
            return;
        }

        var markingTracker = __instance.Map?.GetComponent<MarkingTracker>();
        if (markingTracker?.PawnsToEvaluate != null && !markingTracker.PawnsToEvaluate.Contains(__instance))
        {
            markingTracker.PawnsToEvaluate.Add(__instance);
        }

        var pawn = __instance.InnerPawn;

        if (pawn?.Faction != null)
        {
            MarkThatPawn.FactionMaterialCache.Remove(pawn.Faction);
        }

        if (pawn?.Ideo != null)
        {
            MarkThatPawn.IdeoMaterialCache.Remove(pawn.Ideo);
        }
    }
}