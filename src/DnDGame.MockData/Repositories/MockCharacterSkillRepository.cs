using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Skills;

namespace DnDGame.MockData.Repositories;

public class MockCharacterSkillRepository : ICharacterSkillRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockCharacterSkillRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<CharacterSkillUnlock>> GetByPlayerIdAsync(int playerId)
    {
        var rows = _store.CharacterSkillUnlocks.Where(unlock => unlock.PlayerId == playerId).ToList();
        return Task.FromResult<IReadOnlyList<CharacterSkillUnlock>>(rows);
    }

    public Task<CharacterSkillUnlock?> GetAsync(int playerId, int skillDefinitionId)
    {
        var row = _store.CharacterSkillUnlocks.FirstOrDefault(
            unlock => unlock.PlayerId == playerId && unlock.SkillDefinitionId == skillDefinitionId);
        return Task.FromResult(row);
    }

    public Task<CharacterSkillUnlock> AddAsync(CharacterSkillUnlock unlock)
    {
        _store.CharacterSkillUnlocks.Add(unlock);
        return Task.FromResult(unlock);
    }
}
