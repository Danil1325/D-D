using DnDGame.Domain.Entities.Shop;

namespace DnDGame.BusinessLayer.Dtos.Shop;

/// <summary>One item the current player owns, with how many.</summary>
public class InventoryEntryDto : ShopItemDto
{
    public int Quantity { get; init; }

    public static InventoryEntryDto FromDomain(ShopItem item, int quantity) => new()
    {
        Id = item.Id,
        Code = item.Code,
        Name = item.Name,
        Category = item.Category,
        BuyPrice = item.BuyPrice,
        SellPrice = item.SellPrice,
        Quantity = quantity
    };
}
