using DnDGame.Domain.Entities.Shop;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Shop;

/// <summary>One Shop catalog entry, independent of any player.</summary>
public class ShopItemDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ShopItemCategory Category { get; init; }
    public int BuyPrice { get; init; }
    public int SellPrice { get; init; }

    public static ShopItemDto FromDomain(ShopItem item) => new()
    {
        Id = item.Id,
        Code = item.Code,
        Name = item.Name,
        Category = item.Category,
        BuyPrice = item.BuyPrice,
        SellPrice = item.SellPrice
    };
}
