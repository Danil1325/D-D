using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Services.Interfaces;

public interface IExplicitLocationUnlockService
{
    Task<IReadOnlyList<LocationId>> UnlockExplicitLocationsAsync(
        PlayerCharacter character,
        IEnumerable<int> locationIds);
}
