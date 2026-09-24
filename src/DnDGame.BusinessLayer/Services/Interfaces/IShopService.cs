using DnDGame.BusinessLayer.Dtos.Shop;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Catalog + buy/sell side of the Shop feature. This is a Shop-only item model
/// (see ShopItem's remarks on why it isn't unified with Collection's or the
/// separate Inventory screen's item shapes). The Shop has infinite stock — the
/// frontend's per-card "quantity" is decorative and never enforced by its own
/// buy/sell logic, so it isn't modeled here either. Gold is spent/earned only
/// through Buy/Sell; PlayerCharacter.Gold is the single source of truth.
/// </summary>
public interface IShopService
{
    /// <summary>The whole item catalog, independent of any player.</summary>
    Task<IReadOnlyList<ShopItemDto>> GetCatalogAsync();

    /// <summary>
    /// The current player's gold balance and owned items. Resolves the player
    /// server-side via ICurrentPlayerService. Throws DomainException(NOT_FOUND)
    /// if the current player has no character yet.
    /// </summary>
    Task<ShopStateDto> GetStateForCurrentPlayerAsync();

    /// <summary>
    /// Buys one unit of an item for the current player, deducting BuyPrice from
    /// their Gold. Throws NOT_FOUND if the item doesn't exist, or
    /// INSUFFICIENT_GOLD if the character can't afford it.
    /// </summary>
    Task<ShopStateDto> BuyItemForCurrentPlayerAsync(int itemId);

    /// <summary>
    /// Sells one or more items for the current player in a single batch,
    /// crediting the total SellPrice to their Gold. All-or-nothing: if any line
    /// can't be fulfilled (item not owned, or quantity exceeds what's owned),
    /// nothing is sold. Throws VALIDATION_ERROR if the request is empty or a
    /// line's quantity isn't positive, NOT_FOUND if an item doesn't exist, or
    /// INSUFFICIENT_ITEM_QUANTITY if a line sells more than is owned.
    /// </summary>
    Task<ShopStateDto> SellItemsForCurrentPlayerAsync(IReadOnlyList<SellItemRequestLineDto> items);
}
