namespace DnDGame.BusinessLayer.Models;

/// <summary>Result of resolving the EXP reward for one encounter.</summary>
public sealed record CombatExperienceResult(
    int BaseExperience,
    ExperienceResult? Progression = null,
    bool AlreadyGranted = false,
    bool RepeatedSummon = false)
{
    public int ExperienceGained => Progression?.ExperienceGained ?? 0;
    public bool WasGranted => Progression is not null;
}
