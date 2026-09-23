using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Characters;

/// <summary>
/// Request shape for POST /api/character/new-game. The client supplies only the
/// name, the race (RaceType, whose numeric values align with the seeded Race ids)
/// and the class reference-data id — the starting attributes are rolled server-side
/// from the chosen race's RaceAttributeRange and must never be sent by the client.
/// </summary>
public class NewGameCharacterRequestDto
{
    public string Name { get; set; } = string.Empty;

    public RaceType Race { get; set; }

    /// <summary>
    /// CharacterClass reference-data id (Healer=1, Warrior=2, Magician=3, Bard=4).
    /// Classes are deliberately reference data rather than an enum, so this stays a
    /// repository-validated int instead of a ClassType value.
    /// </summary>
    public int ClassId { get; set; }
}