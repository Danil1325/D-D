using DndGame.BusinessLayer.Interfaces;
using DndGame.Domain.Entities;
using DndGame.DataAccesLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DndGame.DataAccesLayer.Repositories;

public sealed class CharacterRepository(DndGameDbContext dbContext) : ICharacterRepository
{
    public async Task<IReadOnlyList<Character>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Characters.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Characters.FirstOrDefaultAsync(character => character.Id == id, cancellationToken);

    public Task AddAsync(Character character, CancellationToken cancellationToken)
    {
        dbContext.Characters.Add(character);
        return Task.CompletedTask;
    }

    public void Update(Character character) => dbContext.Entry(character).State = EntityState.Modified;

    public void Delete(Character character) => dbContext.Characters.Remove(character);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
