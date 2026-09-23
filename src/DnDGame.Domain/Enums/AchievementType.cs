namespace DnDGame.Domain.Enums;

/// <summary>
/// The event a given Achievement tracks. Each value maps to exactly one
/// application-level write path (see AchievementService's Register* methods):
/// achievements only ever advance from those real game events, never from
/// read-only endpoints (a page view cannot unlock anything).
/// </summary>
public enum AchievementType
{
    /// <summary>Advance on character creation (CharacterService.CreateNewGameAsync).</summary>
    CharacterCreated = 1,

    /// <summary>Advance once per quest completed (QuestService.FinalizeCompletedQuestAsync).</summary>
    QuestsCompleted = 2,

    /// <summary>Advance once per battle won (BattleService.AwardCombatExperienceAsync).</summary>
    BattlesWon = 3,

    /// <summary>Advance once per location unlocked (LocationProgress Locked -> Available).</summary>
    LocationsUnlocked = 4
}
