namespace DnDGame.BusinessLayer.Models;

public sealed record ExperienceResult(
    int PreviousLevel,
    int CurrentLevel,
    int ExperienceGained,
    int TotalExperience,
    int SkillPointsGained)
{
    public int LevelsGained => CurrentLevel - PreviousLevel;
    public bool DidLevelUp => LevelsGained > 0;
}
