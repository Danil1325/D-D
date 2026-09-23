using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.BusinessLayer.Dtos.Characters;

/// <summary>
/// Response shape for character creation (POST /api/character/new-game, 201) and
/// lookup (GET /api/character/{id}). Carries the race/class display data read from
/// reference data plus the starting attributes rolled at creation.
/// </summary>
public class CharacterResponseDto
{
    public int Id { get; init; }

    /// <summary>
    /// The id of the player/account this character belongs to (its OwnerId). Every
    /// New Game screen navigation and the frontend's "resume" flow keys progress by
    /// this value, so the creation response returns it alongside <see cref="Id"/>.
    /// </summary>
    public int PlayerId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int RaceId { get; init; }
    public string RaceName { get; init; } = string.Empty;

    public int ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;

    /// <summary>The class's primary attribute, serialized as text (e.g. "Strength").</summary>
    public string PrimaryAttribute { get; init; } = string.Empty;

    /// <summary>The (Race, Class) portrait image path, resolved from CharacterPortrait.</summary>
    public string? PortraitPath { get; init; }

    public int Level { get; init; }
    public int CurrentXp { get; init; }
    public int SkillPoints { get; init; }

    public int MaxHealth { get; init; }
    public int CurrentHealth { get; init; }

    public int Strength { get; init; }
    public int Dexterity { get; init; }
    public int Intelligence { get; init; }
    public int Charisma { get; init; }

    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Maps a created character to its response shape. <paramref name="portraitPath"/>
    /// is supplied by the service (resolved from CharacterPortrait for (RaceId, ClassId)),
    /// because the portrait is deliberately not stored on PlayerCharacter.
    /// </summary>
    public static CharacterResponseDto FromDomain(PlayerCharacter character, string? portraitPath) => new()
    {
        Id = character.Id,

        // OwnerId stores the current player's id as text (resolved server-side from
        // ICurrentPlayerService — never client-supplied, never a hard-coded "1"), so
        // the integer player id the New Game screen keys progress by is parsed back
        // here. A legacy row carrying an unparseable OwnerId is a corruption, not a
        // "player 0" — surface it loudly instead of masking it with a default id.
        PlayerId = int.TryParse(character.OwnerId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var playerId)
            ? playerId
            : throw new DomainException(ErrorCodes.InternalError, $"Character {character.Id} has an invalid internal player reference '{character.OwnerId}'."),
        Name = character.Name,
        RaceId = character.RaceId,
        RaceName = character.Race?.Name ?? string.Empty,
        ClassId = character.ClassId,
        ClassName = character.CharacterClass?.Name ?? string.Empty,
        PrimaryAttribute = character.CharacterClass?.PrimaryAttribute.ToString() ?? string.Empty,
        PortraitPath = portraitPath,
        Level = character.Level,
        CurrentXp = character.CurrentXp,
        SkillPoints = character.SkillPoints,
        MaxHealth = character.MaxHealth,
        CurrentHealth = character.CurrentHealth,
        Strength = character.Strength,
        Dexterity = character.Dexterity,
        Intelligence = character.Intelligence,
        Charisma = character.Charisma,
        CreatedAt = character.CreatedAt
    };
}