using DndGame.BusinessLayer.Interfaces;
using DndGame.Domain.Entities;

namespace DndGame.BusinessLayer.Services;

public sealed class CharacterService(ICharacterRepository repository) : ICharacterService
{
    public Task<IReadOnlyList<Character>> GetAllAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<Character> CreateAsync(Character character, CancellationToken cancellationToken)
    {
        await repository.AddAsync(character, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return character;
    }

    public async Task<bool> UpdateAsync(int id, Character character, CancellationToken cancellationToken)
    {
        if (id != character.Id || await repository.GetByIdAsync(id, cancellationToken) is null)
        {
            return false;
        }

        repository.Update(character);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var character = await repository.GetByIdAsync(id, cancellationToken);
        if (character is null)
        {
            return false;
        }

        repository.Delete(character);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
