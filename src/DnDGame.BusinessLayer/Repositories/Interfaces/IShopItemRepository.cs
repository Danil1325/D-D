using DnDGame.Domain.Entities.Shop;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>The seeded Shop item catalog (reference data).</summary>
public interface IShopItemRepository
{
    Task<IReadOnlyList<ShopItem>> GetAllAsync();
    Task<ShopItem?> GetByIdAsync(int id);
}
