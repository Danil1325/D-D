using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Classes;

/// <summary>
/// A playable class (Warrior, Magician, Bard, Healer). Reference data — seeded once,
/// never created or edited by players.
/// </summary>
public class CharacterClass : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>The class's role/flavor text (e.g. "Frontline fighters who excel in combat...").</summary>
    public string Description { get; set; } = string.Empty;

    public string Quote { get; set; } = string.Empty;

    /// <summary>Purely decorative tarot-style ordinal — display only, no gameplay meaning.</summary>
    public int CardNumber { get; set; }

    /// <summary>
    /// Which attribute this class uses for its attacks and primary checks
    /// (e.g. Warrior -> Strength, Magician -> Intelligence).
    /// </summary>
    public AttributeType PrimaryAttribute { get; set; }

    /// <summary>
    /// Base damage this class deals on a successful attack ("successful attacks deal
    /// a simple DamageAmount"). Design decision (Phase 1): the approved plan defined
    /// this rule for enemies explicitly but did not name where the player's side of
    /// it lives — this field is that missing piece. Flagged for your review.
    /// </summary>
    public int BaseDamageAmount { get; set; }

    public string ImageFrontPath { get; set; } = string.Empty;
    public string ImageBackPath { get; set; } = string.Empty;

    /// <summary>Flavor text describing armor this class typically wears. Not mechanically enforced.</summary>
    public string ArmorDescription { get; set; } = string.Empty;

    /// <summary>Flavor text describing weapons this class typically uses. Not mechanically enforced.</summary>
    public string WeaponDescription { get; set; } = string.Empty;

    /// <summary>Flavor text describing tools this class is trained with. Not mechanically enforced.</summary>
    public string ToolDescription { get; set; } = string.Empty;

    public ICollection<ClassFeature> Features { get; set; } = new List<ClassFeature>();
}
