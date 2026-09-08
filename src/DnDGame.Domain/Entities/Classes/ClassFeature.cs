using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Classes;

/// <summary>
/// One class feature unlocked at a given level (e.g. Warrior's "Second Wind",
/// Magician's "Arcane Recovery"). Descriptive/flavor for now — see RaceTrait's
/// comments for the same reasoning.
/// </summary>
public class ClassFeature : BaseEntity
{
    public int ClassId { get; set; }
    public CharacterClass? CharacterClass { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>The level at which this feature is unlocked. Defaults to 1.</summary>
    public int LevelAcquired { get; set; } = 1;
}
