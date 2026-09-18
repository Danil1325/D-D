using DnDGame.BusinessLayer.Dtos.Progression;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Character level/experience progression, keyed by player id. The skill tree is
/// deliberately out of scope for now — only accumulated, unspent skill points are
/// reported.
/// </summary>
public interface IProgressionService
{
    /// <summary>Current progression snapshot for the player's character.</summary>
    Task<CharacterProgressionDto> GetProgressionAsync(int playerId);

    /// <summary>
    /// Adds a positive amount of experience to the player's character, applies any
    /// resulting level-ups and returns the updated progression snapshot.
    /// </summary>
    Task<CharacterProgressionDto> GrantExperienceAsync(int playerId, int experience);
}