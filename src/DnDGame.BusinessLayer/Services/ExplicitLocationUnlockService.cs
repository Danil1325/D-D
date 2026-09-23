using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Services;

public sealed class ExplicitLocationUnlockService : IExplicitLocationUnlockService
{
    private readonly ILocationDefinitionRepository _locationDefinitionRepository;
    private readonly ILocationProgressRepository _locationProgressRepository;
    private readonly IAchievementService _achievementService;

    public ExplicitLocationUnlockService(
        ILocationDefinitionRepository locationDefinitionRepository,
        ILocationProgressRepository locationProgressRepository,
        IAchievementService achievementService)
    {
        _locationDefinitionRepository = locationDefinitionRepository;
        _locationProgressRepository = locationProgressRepository;
        _achievementService = achievementService;
    }

    public async Task<IReadOnlyList<LocationId>> UnlockExplicitLocationsAsync(
        PlayerCharacter character,
        IEnumerable<int> locationIds)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(locationIds);

        var requestedLocationIds = locationIds
            .Distinct()
            .Select(ToLocationId)
            .ToList();

        if (requestedLocationIds.Count == 0)
        {
            return Array.Empty<LocationId>();
        }

        foreach (var locationId in requestedLocationIds)
        {
            _ = await _locationDefinitionRepository.GetByIdAsync(locationId)
                ?? throw new DomainException(ErrorCodes.NotFound, $"Location {locationId} was not found.");
        }

        var newLocationIds = new List<LocationId>();
        foreach (var locationId in requestedLocationIds)
        {
            var existing = await _locationProgressRepository.GetAsync(character.Id, locationId);
            if (existing is null)
            {
                await _locationProgressRepository.AddAsync(new LocationProgress
                {
                    PlayerId = character.Id,
                    LocationId = locationId,
                    Status = LocationStatus.Available,
                    UnlockedAtLevel = character.Level
                });
                newLocationIds.Add(locationId);

                // A brand-new row is a Locked -> Available unlock — the event the
                // locations-unlocked achievements track (see QuestService.Upsert... for
                // the engine-driven counterpart that uses the same guard).
                await _achievementService.RegisterLocationUnlockedAsync(character.Id, locationId);
                continue;
            }

            if (existing.Status != LocationStatus.Locked)
            {
                continue;
            }

            existing.Status = LocationStatus.Available;
            existing.UnlockedAtLevel ??= character.Level;
            await _locationProgressRepository.UpdateAsync(existing);
            newLocationIds.Add(locationId);
            await _achievementService.RegisterLocationUnlockedAsync(character.Id, locationId);
        }

        return newLocationIds;
    }

    private static LocationId ToLocationId(int locationId)
    {
        if (!Enum.IsDefined(typeof(LocationId), locationId))
        {
            throw new DomainException(ErrorCodes.ValidationError, $"Location {locationId} does not exist.");
        }

        return (LocationId)locationId;
    }
}
