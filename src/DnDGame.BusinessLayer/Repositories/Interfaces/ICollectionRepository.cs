using DnDGame.Domain.Entities.Collection;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>The seeded Collection catalog (reference data).</summary>
public interface ICollectionRepository
{
    Task<IReadOnlyList<CollectionEntry>> GetAllAsync();
}
