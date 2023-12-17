using System;
using Mlie;
using UnityEngine;
using Verse;

namespace MarkThatPawn;

[StaticConstructorOnStartup]
internal class MarkThatPawnMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static MarkThatPawnMod instance;

    private static string currentVersion;

    private static Vector2 optionsScrollPosition;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public MarkThatPawnMod(ModContentPack content) : base(content)
    {
        instance = this;
        Settings = GetSettings<MarkThatPawnSettings>();
        if (Settings.AutoRuleBlobs == null)
        {
            Settings.AutoRuleBlobs = [];
        }

        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings.AutoRules = [];
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal MarkThatPawnSettings Settings { get; }

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
        var containingRect = rect.ContractedBy(100, 0).CenteredOnXIn(rect);
        var viewingRect = containingRect.ContractedBy(10f);
        viewingRect.height *= 1.85f;
        Widgets.BeginScrollView(containingRect, ref optionsScrollPosition, viewingRect);
        var listing_Standard = new Listing_Standard();

        listing_Standard.Begin(viewingRect);

        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("MTP.RefreshRules".Translate(), ref Settings.RefreshRules,
            "MTP.RefreshRulesTT".Translate());
        listing_Standard.CheckboxLabeled("MTP.PulsatingIcons".Translate(), ref Settings.PulsatingIcons,
            "MTP.PulsatingIconsTT".Translate());
        listing_Standard.CheckboxLabeled("MTP.RelativeIconSize".Translate(), ref Settings.RelativeIconSize,
            "MTP.RelativeIconSizeTT".Translate());
        listing_Standard.CheckboxLabeled("MTP.RelativeToZoom".Translate(), ref Settings.RelativeToZoom,
            "MTP.RelativeToZoomTT".Translate());
        if (Settings.RelativeToZoom)
        {
            Settings.IconScalingFactor =
                (float)Math.Round(listing_Standard.SliderLabeled(
                    "MTP.IconScalingFactor".Translate(Settings.IconScalingFactor.ToStringPercent()),
                    Settings.IconScalingFactor, 0.1f, 5f), 2);
        }

        Settings.IconSize =
            (float)Math.Round(listing_Standard.SliderLabeled(
                "MTP.IconSize".Translate(Settings.IconSize.ToStringPercent()),
                Settings.IconSize, 0.1f, 5f), 1);
        Settings.XOffset =
            listing_Standard.SliderLabeled(
                "MTP.XOffset".Translate(Math.Round(Settings.XOffset, 2)),
                Settings.XOffset, -1f, 1f);
        Settings.ZOffset =
            listing_Standard.SliderLabeled(
                "MTP.ZOffset".Translate(Math.Round(Settings.ZOffset, 2)),
                Settings.ZOffset, -1f, 1f);

        var activeRules = Settings.AutoRules.Count(rule => rule.Enabled);
        if (listing_Standard.ButtonTextLabeled("MTP.RulesButtonInfo".Translate(activeRules),
                "MTP.RulesButtonText".Translate()))
        {
            Find.WindowStack.Add(new Dialog_AutoMarkingRules());
        }

        listing_Standard.GapLine();
        var selectorRect = listing_Standard.GetRect(64f);
        Widgets.Label(selectorRect.LeftHalf(),
            "MTP.DefaultMarkerSet".Translate(Settings.DefaultMarkerSet.LabelCap));
        if (MarkerSelector(selectorRect.RightHalf(), Settings.DefaultMarkerSet))
        {
            Find.WindowStack.Add(
                new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Default)));
        }

        listing_Standard.GapLine();
        listing_Standard.CheckboxLabeled("MTP.ShowForColonist".Translate(), ref Settings.ShowForColonist);
        if (Settings.ShowForColonist)
        {
            selectorRect = listing_Standard.GetRect(64f);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.ColonistDiffer".Translate(Settings.ColonistMarkerSet.LabelCap), ref Settings.ColonistDiffer);
            if (Settings.ColonistDiffer)
            {
                if (MarkerSelector(selectorRect.RightHalf(), Settings.ColonistMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Colonist)));
                }
            }
        }

        listing_Standard.GapLine();
        listing_Standard.CheckboxLabeled("MTP.ShowForPrisoner".Translate(), ref Settings.ShowForPrisoner);
        if (Settings.ShowForPrisoner)
        {
            selectorRect = listing_Standard.GetRect(64f);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.PrisonerDiffer".Translate(Settings.PrisonerMarkerSet.LabelCap), ref Settings.PrisonerDiffer);
            if (Settings.PrisonerDiffer)
            {
                if (MarkerSelector(selectorRect.RightHalf(), Settings.PrisonerMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Prisoner)));
                }
            }
        }

        if (ModLister.RoyaltyInstalled)
        {
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("MTP.ShowForSlave".Translate(), ref Settings.ShowForSlave);
            if (Settings.ShowForSlave)
            {
                selectorRect = listing_Standard.GetRect(64f);
                Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                    "MTP.SlaveDiffer".Translate(Settings.SlaveMarkerSet.LabelCap), ref Settings.SlaveDiffer);
                if (Settings.SlaveDiffer)
                {
                    if (MarkerSelector(selectorRect.RightHalf(), Settings.SlaveMarkerSet))
                    {
                        Find.WindowStack.Add(
                            new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Slave)));
                    }
                }
            }
        }

        listing_Standard.GapLine();
        listing_Standard.CheckboxLabeled("MTP.ShowForEnemy".Translate(), ref Settings.ShowForEnemy);
        if (Settings.ShowForEnemy)
        {
            selectorRect = listing_Standard.GetRect(64f);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.EnemyDiffer".Translate(Settings.EnemyMarkerSet.LabelCap), ref Settings.EnemyDiffer);
            if (Settings.EnemyDiffer)
            {
                if (MarkerSelector(selectorRect.RightHalf(), Settings.EnemyMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Enemy)));
                }
            }
        }

        listing_Standard.GapLine();
        listing_Standard.CheckboxLabeled("MTP.ShowForNeutral".Translate(), ref Settings.ShowForNeutral);
        if (Settings.ShowForNeutral)
        {
            selectorRect = listing_Standard.GetRect(64f);
            Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                "MTP.NeutralDiffer".Translate(Settings.NeutralMarkerSet.LabelCap), ref Settings.NeutralDiffer);
            if (Settings.NeutralDiffer)
            {
                if (MarkerSelector(selectorRect.RightHalf(), Settings.NeutralMarkerSet))
                {
                    Find.WindowStack.Add(
                        new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Neutral)));
                }
            }
        }

        if (MarkThatPawn.VehiclesLoaded)
        {
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("MTP.ShowForVehicles".Translate(), ref Settings.ShowForVehicles);
            if (Settings.ShowForVehicles)
            {
                selectorRect = listing_Standard.GetRect(64f);
                Widgets.CheckboxLabeled(selectorRect.LeftHalf().TopHalf(),
                    "MTP.VehiclesDiffer".Translate(Settings.VehiclesMarkerSet.LabelCap), ref Settings.VehiclesDiffer);
                if (Settings.VehiclesDiffer)
                {
                    if (MarkerSelector(selectorRect.RightHalf(), Settings.VehiclesMarkerSet))
                    {
                        Find.WindowStack.Add(
                            new FloatMenu(MarkThatPawn.GetMarkingSetOptions(MarkThatPawn.PawnType.Vehicle)));
                    }
                }
            }
        }

        if (listing_Standard.ButtonText("MTP.Reset".Translate()))
        {
            Settings.Reset();
        }

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("MTP.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
        Widgets.EndScrollView();
        MarkThatPawn.ResetCache();
    }

    public static bool MarkerSelector(Rect rowRect, MarkerDef marker)
    {
        var imageRect = rowRect;
        var buttonRect = rowRect.LeftHalf().RightPart(0.75f);
        buttonRect.height = 25f;
        buttonRect = buttonRect.CenteredOnYIn(rowRect);
        imageRect.x = imageRect.xMax - imageRect.height;
        imageRect.width = imageRect.height;
        GUI.DrawTexture(imageRect, marker.Icon);
        TooltipHandler.TipRegion(buttonRect, marker.description);
        return Widgets.ButtonText(buttonRect, marker.LabelCap);
    }
}