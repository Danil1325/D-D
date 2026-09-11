namespace DnDGame.Domain.Enums;

/// <summary>Controls which details of a locked card can be shown to its owner.</summary>
public enum CardVisibilityRule
{
    HideAll,
    ShowNameOnly,
    HideStatistics,
    ShowBasicInformation
}
