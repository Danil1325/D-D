namespace DnDGame.BusinessLayer.Dtos.Shop;

/// <summary>One line of a sell request: how many of one item to sell.</summary>
public class SellItemRequestLineDto
{
    public int ItemId { get; init; }
    public int Quantity { get; init; }
}
