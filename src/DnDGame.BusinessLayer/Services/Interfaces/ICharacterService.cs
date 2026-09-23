using DnDGame.BusinessLayer.Dtos.Characters;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Character-creation surface: creates the single character allowed per player
/// (New Game), serves the race/class reference-data options for the New Game
/// screen, and looks characters up by id.
/// </summary>
public interface ICharacterService
{
    /// <summary>
    /// Creates a character for the current player (Name + RaceType + ClassId).
    /// Starting attributes and health are rolled server-side from the race's
    /// RaceAttributeRange; the player may only ever create one character.
    /// </summary>
    Task<CharacterResponseDto> CreateNewGameAsync(NewGameCharacterRequestDto request);

    /// <summary>The seeded races and classes for the New Game selection screen.</summary>
    Task<CharacterOptionsDto> GetOptionsAsync();

    /// <summary>Returns the character with the given id, or null if it does not exist.</summary>
    Task<CharacterResponseDto?> GetByIdAsync(int id);
}