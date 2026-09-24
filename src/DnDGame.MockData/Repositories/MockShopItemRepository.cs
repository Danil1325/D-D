using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Shop;

namespace DnDGame.MockData.Repositories;

public class MockShopItemRepository : IShopItemRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockShopItemRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<ShopItem>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<ShopItem>>(_store.ShopItems.ToList());
    }

    public Task<ShopItem?> GetByIdAsync(int id)
    {
        var item = _store.ShopItems.FirstOrDefault(candidate => candidate.Id == id);
        return Task.FromResult(item);
    }
}
