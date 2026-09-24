namespace DnDGame.BusinessLayer.Common.Errors;

/// <summary>
/// Error codes specific to the Shop buy/sell flow. Kept separate from the
/// generic ErrorCodes and the other per-feature code classes, following the
/// same per-feature convention as SkillErrorCodes.
/// </summary>
public static class ShopErrorCodes
{
    /// <summary>The character does not have enough Gold to buy the requested item.</summary>
    public const string InsufficientGold = "INSUFFICIENT_GOLD";

    /// <summary>The character does not own enough of the requested item to sell that quantity.</summary>
    public const string InsufficientItemQuantity = "INSUFFICIENT_ITEM_QUANTITY";
}
