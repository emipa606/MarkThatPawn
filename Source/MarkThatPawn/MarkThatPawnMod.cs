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

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public MarkThatPawnMod(ModContentPack content) : base(content)
    {
        instance = this;
        Settings = GetSettings<MarkThatPawnSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal MarkThatPawnSettings Settings { get; }

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
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("MTP.PulsatingIcons".Translate(), ref Settings.PulsatingIcons,
            "MTP.PulsatingIconsTT".Translate());
        Settings.IconSize =
            (float)Math.Round(listing_Standard.SliderLabeled(
                "MTP.IconSize".Translate(Settings.IconSize.ToStringPercent()),
                Settings.IconSize, 0.1f, 2.5f), 1);
        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("MTP.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }
}