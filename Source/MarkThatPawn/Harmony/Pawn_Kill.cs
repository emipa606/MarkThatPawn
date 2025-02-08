using HarmonyLib;
using Verse;

namespace MarkThatPawn.Harmony;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class Pawn_Kill
{
    public static void Postfix(Pawn __instance)
    {
        if (__instance.Corpse == null)
        {
            return;
        }

        var globalMarkingTracker = Current.Game.GetComponent<GlobalMarkingTracker>();
        globalMarkingTracker?.ReplacePawnWithCorpse(__instance, __instance.Corpse);
    }
}