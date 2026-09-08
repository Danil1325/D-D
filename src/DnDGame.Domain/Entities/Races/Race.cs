using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Races;

/// <summary>
/// A playable race (Human, Elf, Orc, Dwarf). Reference data — seeded once by
/// MockData/DataAccessLayer, never created or edited by players.
/// </summary>
public class Race : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Flavor intro text shown on the race's card.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>The italic quote at the bottom of the race's back (stat) card.</summary>
    public string Quote { get; set; } = string.Empty;

    /// <summary>
    /// Purely decorative tarot-style ordinal from the art cards (Human=1, Elf=2,
    /// Orc=3, Dwarf=4). Display only — no gameplay meaning.
    /// </summary>
    public int CardNumber { get; set; }

    /// <summary>Path to the race's artwork card image, e.g. "races/orc_front.png".</summary>
    public string ImageFrontPath { get; set; } = string.Empty;

    /// <summary>Path to the race's stat/description card image, e.g. "races/orc_back.png".</summary>
    public string ImageBackPath { get; set; } = string.Empty;

    /// <summary>Flavor traits shown on the race's card (e.g. "Wild Angry", "Darkvision").</summary>
    public ICollection<RaceTrait> Traits { get; set; } = new List<RaceTrait>();

    /// <summary>
    /// The min/max roll range for each of the 5 attributes when a character of this
    /// race is created. This is where a race's mechanical bonus actually lives — e.g.
    /// Orc's "+2 Strength" flavor is reflected by giving Orc a higher Strength range
    /// than other races, not by a bonus field bolted onto a trait.
    /// </summary>
    public ICollection<RaceAttributeRange> AttributeRanges { get; set; } = new List<RaceAttributeRange>();
}
