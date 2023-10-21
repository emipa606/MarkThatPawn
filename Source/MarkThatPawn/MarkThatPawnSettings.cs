using Verse;

namespace MarkThatPawn;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class MarkThatPawnSettings : ModSettings
{
    public float IconSize = 0.7f;
    public bool PulsatingIcons;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref PulsatingIcons, "PulsatingIcons");
        Scribe_Values.Look(ref IconSize, "IconSize", 0.7f);
    }
}