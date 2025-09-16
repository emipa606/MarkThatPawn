using System;
using Mlie;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

[StaticConstructorOnStartup]
public class MarkThatPawnMod : Mod
{
    private const float SelectorHeight = 50f;

    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static MarkThatPawnMod Instance;

    private static string currentVersion;

    //private static Vector2 optionsScrollPosition;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public MarkThatPawnMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<MarkThatPawnSettings>();
        Settings.AutoRuleBlobs ??= [];

        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings.AutoRules = [];
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    public MarkThatPawnSettings Settings { get; }

    public override void WriteSettings()
    {
        Settings.AutoRuleBlobs = [];
        foreach (var rule in Settings.AutoRules)
        {
            Settings.AutoRuleBlobs.Add(rule.GetBlob());
        }

        base.WriteSettings();
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Mark That Pawn";
    }


    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();

        listingStandard.Begin(rect);
        listingStandard.ColumnWidth = rect.width * 0.48f;
        listingStandard.Gap();
        listingStandard.CheckboxLabeled("MTP.RefreshRules".Translate(), ref Settings.RefreshRules,
            "MTP.RefreshRulesTT".Translate());
        listingStandard.CheckboxLabeled("MTP.ShowOnCorpses".Translate(), ref Settings.ShowOnCorpses,
            "MTP.ShowOnCorpsesTT".Translate());
        listingStandard.CheckboxLabeled("MTP.SeparateTemporary".Translate(), ref Settings.SeparateTemporary,
            "MTP.SeparateTemporaryTT".Translate());
        if (Settings.SeparateTemporary)
        {
            listingStandard.CheckboxLabeled("MTP.RotateIcons".Translate(), ref Settings.RotateIcons,
                "MTP.RotateIconsTT".Translate());
            listingStandard.CheckboxLabeled("MTP.SeparateShowAll".Translate(), ref Settings.SeparateShowAll,
                "MTP.SeparateShowAllTT".Translate());
            listingStandard.CheckboxLabeled("MTP.NormalShowAll".Translate(), ref Settings.NormalShowAll,
                "MTP.NormalShowAllTT".Translate());
            listingStandard.CheckboxLabeled("MTP.InvertOrder".Translate(), ref Settings.InvertOrder,
                "MTP.InvertOrderTT".Translate());
            if (!Settings.RotateIcons)
            {
                Settings.IconSpacingFactor =
                    listingStandard.SliderLabeled(
                        "MTP.IconSpacingFactor".Translate(Settings.IconSpacingFactor.ToStringPercent()),
                        Settings.IconSpacingFactor, -1f, 1f);
            }

            if (Settings.IconSpacingFactor < 0f || Settings.RotateIcons)
            {
                listingStandard.CheckboxLabeled("MTP.ShowWhenSelected".Translate(), ref Settings.ShowWhenSelected,
                    "MTP.ShowWhenSelectedTT".Translate());
                listingStandard.CheckboxLabeled("MTP.ShowWhenHover".Translate(), ref Settings.ShowWhenHover,
                    "MTP.ShowWhenHoverTT".Translate());
                listingStandard.CheckboxLabeled("MTP.ShowOnShift".Translate(), ref Settings.ShowOnShift,
                    "MTP.ShowOnShiftTT".Translate());
                listingStandard.CheckboxLabeled("MTP.ShowOnPaused".Translate(), ref Settings.ShowOnPaused,
                    "MTP.ShowOnPausedTT".Translate());
            }
            else
            {
                Settings.ShowWhenSelected = false;
            }
        }

        listingStandard.Label("MTP.HideIcons".Translate());
        listingStandard.CheckboxLabeled("MTP.PawnIsSelected".Translate(), ref Settings.PawnIsSelected);
        listingStandard.CheckboxLabeled("MTP.ShiftIsPressed".Translate(), ref Settings.ShiftIsPressed);
        listingStandard.CheckboxLabeled("MTP.GameIsPaused".Translate(), ref Settings.GameIsPaused);

        listingStandard.CheckboxLabeled("MTP.PulsatingIcons".Translate(), ref Settings.PulsatingIcons,
            "MTP.PulsatingIconsTT".Translate());
        listingStandard.CheckboxLabeled("MTP.RelativeIconSize".Translate(), ref Settings.RelativeIconSize,
            "MTP.RelativeIconSizeTT".Translate());
        listingStandard.CheckboxLabeled("MTP.RelativeToZoom".Translate(), ref Settings.RelativeToZoom,
            "MTP.RelativeToZoomTT".Translate());
        if (Settings.RelativeToZoom)
        {
            Settings.IconScalingFactor =
                (float)Math.Round(listingStandard.SliderLabeled(
                    "MTP.IconScalingFactor".Translate(Settings.IconScalingFactor.ToStringPercent()),
                    Settings.IconScalingFactor, 0.1f, 5f), 2);
        }

        Settings.IconSize =
            (float)Math.Round(listingStandard.SliderLabeled(
                "MTP.IconSize".Translate(Settings.IconSize.ToStringPercent()),
                Settings.IconSize, 0.1f, 5f), 1);
        Settings.XOffset =
            listingStandard.SliderLabeled(
                "MTP.XOffset".Translate(Math.Round(Settings.XOffset, 2)),
                Settings.XOffset, -1f, 1f);
        Settings.ZOffset =
            listingStandard.SliderLabeled(
                "MTP.ZOffset".Translate(Math.Round(Settings.ZOffset, 2)),
                Settings.ZOffset, -1f, 1f);

        listingStandard.NewColumn();
        var activeRules = Settings.AutoRules.Count(rule => rule.Enabled);
        if (listingStandard.ButtonTextLabeled("MTP.RulesButtonInfo".Translate(activeRules),
                "MTP.RulesButtonText".Translate()))
        {
            Find.WindowStack.Add(new Dialog_AutoMarkingRules());
        }

        var selectorRect = listingStandard.GetRect(SelectorHeight);
        Widgets.Label(selectorRect.LeftHalf(),
            "MTP.DefaultMarkerSet".Translate(Settings.DefaultMarkerSet.LabelCap));
        if (markerSelector(selectorRect.RightHalf(), Settings.DefaultMarkerSet))
        {
            Find.WindowStack.Add(
                new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Default)));
        }

        listingStandard.GapLine();
        listingStandard.CheckboxLabeled("MTP.ShowForColonist".Translate(), ref Settings.ShowForColonist);
        if (Settings.ShowForColonist)
        {
            selectorRect = listingStandard.GetRect(SelectorHeight);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.ColonistDiffer".Translate(Settings.ColonistMarkerSet.LabelCap), ref Settings.ColonistDiffer);
            if (Settings.ColonistDiffer)
            {
                if (markerSelector(selectorRect.RightHalf(), Settings.ColonistMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Colonist)));
                }
            }
        }

        listingStandard.GapLine();
        listingStandard.CheckboxLabeled("MTP.ShowForPrisoner".Translate(), ref Settings.ShowForPrisoner);
        if (Settings.ShowForPrisoner)
        {
            selectorRect = listingStandard.GetRect(SelectorHeight);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.PrisonerDiffer".Translate(Settings.PrisonerMarkerSet.LabelCap), ref Settings.PrisonerDiffer);
            if (Settings.PrisonerDiffer)
            {
                if (markerSelector(selectorRect.RightHalf(), Settings.PrisonerMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Prisoner)));
                }
            }
        }

        if (ModLister.RoyaltyInstalled)
        {
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("MTP.ShowForSlave".Translate(), ref Settings.ShowForSlave);
            if (Settings.ShowForSlave)
            {
                selectorRect = listingStandard.GetRect(SelectorHeight);
                Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                    "MTP.SlaveDiffer".Translate(Settings.SlaveMarkerSet.LabelCap), ref Settings.SlaveDiffer);
                if (Settings.SlaveDiffer)
                {
                    if (markerSelector(selectorRect.RightHalf(), Settings.SlaveMarkerSet))
                    {
                        Find.WindowStack.Add(
                            new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Slave)));
                    }
                }
            }
        }

        listingStandard.GapLine();
        listingStandard.CheckboxLabeled("MTP.ShowForEnemy".Translate(), ref Settings.ShowForEnemy);
        if (Settings.ShowForEnemy)
        {
            selectorRect = listingStandard.GetRect(SelectorHeight);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.EnemyDiffer".Translate(Settings.EnemyMarkerSet.LabelCap), ref Settings.EnemyDiffer);
            if (Settings.EnemyDiffer)
            {
                if (markerSelector(selectorRect.RightHalf(), Settings.EnemyMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Enemy)));
                }
            }
        }

        listingStandard.GapLine();
        listingStandard.CheckboxLabeled("MTP.ShowForNeutral".Translate(), ref Settings.ShowForNeutral);
        if (Settings.ShowForNeutral)
        {
            selectorRect = listingStandard.GetRect(SelectorHeight);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.NeutralDiffer".Translate(Settings.NeutralMarkerSet.LabelCap), ref Settings.NeutralDiffer);
            if (Settings.NeutralDiffer)
            {
                if (markerSelector(selectorRect.RightHalf(), Settings.NeutralMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Neutral)));
                }
            }
        }

        if (MarkThatPawn.VehiclesLoaded)
        {
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("MTP.ShowForVehicles".Translate(), ref Settings.ShowForVehicles);
            if (Settings.ShowForVehicles)
            {
                selectorRect = listingStandard.GetRect(SelectorHeight);
                Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                    "MTP.VehiclesDiffer".Translate(Settings.VehiclesMarkerSet.LabelCap), ref Settings.VehiclesDiffer);
                if (Settings.VehiclesDiffer)
                {
                    if (markerSelector(selectorRect.RightHalf(), Settings.VehiclesMarkerSet))
                    {
                        Find.WindowStack.Add(
                            new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Vehicle)));
                    }
                }
            }
        }

        if (listingStandard.ButtonText("MTP.Reset".Translate()))
        {
            Settings.Reset();
        }

        if (currentVersion != null)
        {
            GUI.contentColor = Color.gray;
            listingStandard.Label("MTP.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
        MarkThatPawn.ResetCache();
    }

    private static bool markerSelector(Rect rowRect, MarkerDef marker)
    {
        var imageRect = rowRect;
        var buttonRect = rowRect.LeftPart(0.75f).RightPart(0.95f);
        buttonRect.height = 25f;
        buttonRect = buttonRect.CenteredOnYIn(rowRect);
        imageRect.x = imageRect.xMax - imageRect.height;
        imageRect.width = imageRect.height;
        GUI.DrawTexture(imageRect, marker.Icon);
        TooltipHandler.TipRegion(buttonRect, marker.description);
        return Widgets.ButtonText(buttonRect, marker.LabelCap);
    }
}