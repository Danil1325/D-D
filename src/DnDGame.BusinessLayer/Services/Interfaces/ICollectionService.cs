using DnDGame.BusinessLayer.Dtos.Collection;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Read side of the Collection feature. The catalog is not scoped to any
/// player and carries no "discovered" state: there is no real game event
/// (a battle win, an item purchase, ...) that would flip such a flag today, so
/// this deliberately stays a plain, always-visible reference catalog rather than
/// inventing an unwired discovery mechanic. Revisit once a real trigger exists.
/// </summary>
public interface ICollectionService
{
    Task<IReadOnlyList<CollectionEntryDto>> GetCatalogAsync();
}
