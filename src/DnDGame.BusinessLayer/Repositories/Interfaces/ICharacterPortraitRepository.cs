using DnDGame.Domain.Entities.Portraits;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to the (Race, Class) -&gt; portrait image lookup table.</summary>
public interface ICharacterPortraitRepository
{
    Task<IReadOnlyList<CharacterPortrait>> GetAllAsync();
    Task<CharacterPortrait?> GetByRaceAndClassAsync(int raceId, int classId);
}
