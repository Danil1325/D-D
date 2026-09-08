using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Classes;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Races;

namespace DnDGame.Domain.Entities.Characters;

/// <summary>
/// A character created and played by a user. Attributes are rolled randomly at
/// creation, within the chosen race's ranges (see RaceAttributeRange) — never
/// assigned by the player, never derived from a standard array.
/// </summary>
public class PlayerCharacter : BaseEntity
{
    /// <summary>
    /// Placeholder owner identifier. There is no real account system until Phase 6
    /// (ASP.NET Identity, introduced alongside the database) — until then every
    /// character just carries a fixed value here.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int RaceId { get; set; }
    public Race? Race { get; set; }

    public int ClassId { get; set; }
    public CharacterClass? CharacterClass { get; set; }

    /// <summary>
    /// Health is both an attribute and the character's hit-point pool — no derivation.
    /// This is the value used whenever a dice check or combat calculation reads
    /// AttributeType.Health — CurrentHealth is never substituted in for it, even
    /// when the character is currently damaged.
    /// </summary>
    public int MaxHealth { get; set; }

    /// <summary>The character's current HP. Fluctuates during play; never used as the Health attribute value in checks.</summary>
    public int CurrentHealth { get; set; }

    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Charisma { get; set; }

    public int Level { get; set; } = 1;
    public int CurrentXp { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<CharacterTalent> Talents { get; set; } = new List<CharacterTalent>();
    public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    // Note: the character's portrait image is deliberately NOT stored on this entity.
    // It is resolved by looking up CharacterPortrait for (RaceId, ClassId) whenever
    // needed, so it can never go stale if a portrait mapping is corrected later.
}
