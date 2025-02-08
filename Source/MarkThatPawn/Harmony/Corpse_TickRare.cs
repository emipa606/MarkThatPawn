using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(Corpse), nameof(Corpse.TickRare))]
public static class Corpse_TickRare
{
    public static void Postfix(Corpse __instance)
    {
        if (!MarkThatPawnMod.instance.Settings.RefreshRules || !MarkThatPawnMod.instance.Settings.ShowOnCorpses ||
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

        if (pawn?.Faction != null && MarkThatPawn.FactionMaterialCache.ContainsKey(pawn.Faction))
        {
            MarkThatPawn.FactionMaterialCache.Remove(pawn.Faction);
        }

        if (pawn?.Ideo != null && MarkThatPawn.IdeoMaterialCache.ContainsKey(pawn.Ideo))
        {
            MarkThatPawn.IdeoMaterialCache.Remove(pawn.Ideo);
        }
    }
}