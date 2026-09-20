namespace DnDGame.BusinessLayer.Models;

/// <summary>
/// Outcome of completing a quest. Experience is never granted more than once per
/// quest: the player quest transitions to <see cref="DnDGame.Domain.Enums.QuestStatus.Completed"/>
/// together with the grant, and completing again fails before touching experience.
/// </summary>
public sealed record QuestCompletionResult(
    int QuestId,
    string QuestTitle,
    int ExperienceGained,
    int PreviousLevel,
    int CurrentLevel,
    int SkillPointsGained)
{
    public IReadOnlyCollection<int> NewLocationIds { get; init; } = Array.Empty<int>();

    public int LevelsGained => CurrentLevel - PreviousLevel;
    public bool DidLevelUp => LevelsGained > 0;
}
