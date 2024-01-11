using System.Collections.Generic;
using TD_Find_Lib;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class GameComponent_TDFindLibRuleComponent : GameComponent
{
    public List<QuerySearch> TDFindLibSearches;

    public GameComponent_TDFindLibRuleComponent(Game game)
    {
    }

    public override void LoadedGame()
    {
        base.LoadedGame();
        foreach (var rule in MarkThatPawnMod.instance.Settings.AutoRules)
        {
            if (rule is not TDFindLibRule tdFindLibRule)
            {
                continue;
            }

            tdFindLibRule.PopulateRuleParameterObjects();
            tdFindLibRule.SetEnabled(true);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Collections.Look(ref TDFindLibSearches, "TDFindLibSearches", LookMode.Deep);
    }
}