using DnDGame.BusinessLayer.Dtos.Collection;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;

namespace DnDGame.BusinessLayer.Services;

/// <summary>Application-service implementation of the Collection catalog. See <see cref="ICollectionService"/>.</summary>
public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _collectionRepository;

    public CollectionService(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<IReadOnlyList<CollectionEntryDto>> GetCatalogAsync()
    {
        var entries = await _collectionRepository.GetAllAsync();
        return entries
            .OrderBy(entry => entry.Id)
            .Select(CollectionEntryDto.FromDomain)
            .ToList();
    }
}
