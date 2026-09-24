using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Shop;

namespace DnDGame.MockData.Repositories;

public class MockCharacterInventoryRepository : ICharacterInventoryRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockCharacterInventoryRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<CharacterInventoryEntry>> GetByPlayerIdAsync(int playerId)
    {
        var rows = _store.CharacterInventoryEntries.Where(entry => entry.PlayerId == playerId).ToList();
        return Task.FromResult<IReadOnlyList<CharacterInventoryEntry>>(rows);
    }

    public Task<CharacterInventoryEntry?> GetAsync(int playerId, int shopItemId)
    {
        var row = _store.CharacterInventoryEntries.FirstOrDefault(
            entry => entry.PlayerId == playerId && entry.ShopItemId == shopItemId);
        return Task.FromResult(row);
    }

    public Task<CharacterInventoryEntry> AddAsync(CharacterInventoryEntry entry)
    {
        _store.CharacterInventoryEntries.Add(entry);
        return Task.FromResult(entry);
    }

    public Task UpdateAsync(CharacterInventoryEntry entry)
    {
        return Task.CompletedTask;
    }
}
