using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Configuration;

/// <summary>
/// Configurable visibility presets for locked cards. Applications can replace a
/// preset while retaining the same <see cref="CardVisibilityRule"/> selection.
/// </summary>
public sealed class CardVisibilityRules
{
    public CardInformationVisibility HideAll { get; init; } = new();

    public CardInformationVisibility ShowNameOnly { get; init; } = new(ShowName: true);

    public CardInformationVisibility HideStatistics { get; init; } = new(
        ShowName: true,
        ShowDescription: true,
        ShowCardType: true,
        ShowCategory: true,
        ShowRarity: true,
        ShowTargetType: true,
        ShowEffectType: true);

    public CardInformationVisibility ShowBasicInformation { get; init; } = new(
        ShowName: true,
        ShowDescription: true,
        ShowCardType: true,
        ShowCategory: true,
        ShowRarity: true);

    public CardInformationVisibility Resolve(CardVisibilityRule rule) => rule switch
    {
        CardVisibilityRule.HideAll => HideAll,
        CardVisibilityRule.ShowNameOnly => ShowNameOnly,
        CardVisibilityRule.HideStatistics => HideStatistics,
        CardVisibilityRule.ShowBasicInformation => ShowBasicInformation,
        _ => throw new ArgumentOutOfRangeException(nameof(rule), rule, "Unknown card visibility rule.")
    };
}

/// <summary>Field-level projection settings used by <see cref="CardVisibilityRules"/>.</summary>
public sealed record CardInformationVisibility(
    bool ShowName = false,
    bool ShowDescription = false,
    bool ShowCardType = false,
    bool ShowCategory = false,
    bool ShowRarity = false,
    bool ShowCost = false,
    bool ShowDamage = false,
    bool ShowTargetType = false,
    bool ShowEffectType = false)
{
    public static CardInformationVisibility Complete { get; } = new(
        ShowName: true,
        ShowDescription: true,
        ShowCardType: true,
        ShowCategory: true,
        ShowRarity: true,
        ShowCost: true,
        ShowDamage: true,
        ShowTargetType: true,
        ShowEffectType: true);
}
