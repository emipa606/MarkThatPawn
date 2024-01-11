using TD_Find_Lib;
using UnityEngine;
using Verse;

namespace MarkThatPawn.MarkerRules;

public class TDFindLibRule : MarkerRule
{
    private QuerySearch tdSearcher;

    public TDFindLibRule()
    {
        RuleType = AutoRuleType.TDFindLib;
        SetDefaultValues();
        RequiresAnActiveGame = true;
        RuleParameters = randomName();
        tdSearcher = new QuerySearch
        {
            name = RuleParameters
        };
        if (Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches == null)
        {
            Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches = [];
        }

        Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches.Add(tdSearcher);
    }

    public TDFindLibRule(string blob)
    {
        RuleType = AutoRuleType.TDFindLib;
        RequiresAnActiveGame = true;
        SetBlob(blob);
    }

    private static string randomName()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var returnValue = "Rule ID: ";
        for (var i = 0; i < 5; i++)
        {
            returnValue += chars.RandomElement();
        }

        returnValue += ". Dont change!";

        return returnValue;
    }

    public override void OnDelete()
    {
        base.OnDelete();
        Current.Game?.GetComponent<GameComponent_TDFindLibRuleComponent>()?.TDFindLibSearches.Remove(tdSearcher);
    }

    public override void ShowTypeParametersRect(Rect rect, bool edit)
    {
        var mentalStateTypeArea = rect.LeftPart(0.75f);
        if (Current.ProgramState != ProgramState.Playing)
        {
            Widgets.Label(mentalStateTypeArea.TopHalf().CenteredOnYIn(rect), "MTP.RequiresAnActiveGame".Translate());
            return;
        }

        if (edit)
        {
            Widgets.Label(mentalStateTypeArea.TopHalf(), tdSearcher.Name);
            if (Widgets.ButtonText(mentalStateTypeArea.BottomHalf(), "MTP.EditTdRule".Translate()))
            {
                Find.WindowStack.Add(new PawnToMarkEditor(tdSearcher));
            }
        }
        else
        {
            Widgets.Label(mentalStateTypeArea.TopHalf().CenteredOnYIn(rect), tdSearcher.Name);
        }
    }

    public override MarkerRule GetCopy()
    {
        var returnRule = new TDFindLibRule(GetBlob())
        {
            RuleParameters = randomName()
        };
        if (Current.ProgramState != ProgramState.Playing)
        {
            return returnRule;
        }

        returnRule.tdSearcher = new QuerySearch
        {
            name = returnRule.RuleParameters,
            parameters = tdSearcher.parameters
        };

        if (Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches == null)
        {
            Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches = [];
        }

        Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches.Add(returnRule.tdSearcher);
        return returnRule;
    }

    public override MarkerRule GetEditableVersion()
    {
        return new TDFindLibRule(GetBlob());
    }

    protected override bool CanEnable()
    {
        return base.CanEnable() && tdSearcher != null && Current.ProgramState == ProgramState.Playing;
    }

    public override void PopulateRuleParameterObjects()
    {
        if (Current.ProgramState != ProgramState.Playing)
        {
            return;
        }

        if (RuleParameters == null)
        {
            return;
        }

        if (RuleParameters == string.Empty && !Enabled)
        {
            return;
        }

        if (Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches == null ||
            Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches.Any() == false)
        {
            ErrorMessage = $"No findLib rules exists to find {RuleParameters}, disabling rule";
            ConfigError = true;
            return;
        }

        foreach (var querySearch in Current.Game.GetComponent<GameComponent_TDFindLibRuleComponent>().TDFindLibSearches)
        {
            if (querySearch.name != RuleParameters)
            {
                continue;
            }

            tdSearcher = querySearch;
            return;
        }

        ErrorMessage = $"Found no saved searcher named {RuleParameters}, disabling rule";
        ConfigError = true;
    }

    public override bool AppliesToPawn(Pawn pawn)
    {
        if (!base.AppliesToPawn(pawn))
        {
            return false;
        }

        if (pawn == null || pawn.Destroyed || !pawn.Spawned || pawn.Map == null)
        {
            return false;
        }

        if (!tdSearcher.Active)
        {
            tdSearcher.active = true;
        }

        tdSearcher.SetSearchMap(pawn.Map);

        return tdSearcher.result.allThings.Contains(pawn);
    }

    public class PawnToMarkEditor : SearchEditorRevertableWindow, ISearchReceiver
    {
        // ISearchReceiver stuff
        public static string TransferTag = "TD.MTP";

        public PawnToMarkEditor(QuerySearch search) : base(search, TransferTag)
        {
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            onlyOneOfTypeAllowed = true;
            draggable = false;
            resizeable = false;
        }

        public override Vector2 InitialSize => new Vector2(750, 320);
        public string Source => TransferTag;
        public string ReceiveName => "MTP.TDFindLibRuleLabel".Translate();
        public QuerySearch.CloneArgs CloneArgs => QuerySearch.CloneArgs.use;

        public bool CanReceive()
        {
            return true;
        }

        public void Receive(QuerySearch search)
        {
            Import(search);
        }

        public override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            windowRect.height = UI.screenHeight / (float)2;
            windowRect.width = UI.screenWidth / (float)2;
            windowRect.x = (UI.screenWidth - windowRect.width) / 2;
            windowRect.y = (UI.screenHeight - windowRect.height) / 2;
        }

        public override void PostOpen()
        {
            base.PostOpen();

            SearchTransfer.Register(this);
        }

        public override void PreClose()
        {
            base.PreClose();

            SearchTransfer.Deregister(this);
        }
    }
}