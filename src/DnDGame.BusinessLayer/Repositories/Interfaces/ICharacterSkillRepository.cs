using DnDGame.Domain.Entities.Skills;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>
/// Per-player unlocked-skill records, composite-keyed (PlayerId, SkillDefinitionId) —
/// the same shape IAchievementProgressRepository has for achievements.
/// </summary>
public interface ICharacterSkillRepository
{
    Task<IReadOnlyList<CharacterSkillUnlock>> GetByPlayerIdAsync(int playerId);
    Task<CharacterSkillUnlock?> GetAsync(int playerId, int skillDefinitionId);
    Task<CharacterSkillUnlock> AddAsync(CharacterSkillUnlock unlock);
}
