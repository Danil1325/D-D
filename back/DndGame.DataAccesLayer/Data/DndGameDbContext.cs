using DndGame.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DndGame.DataAccesLayer.Data;

public sealed class DndGameDbContext(DbContextOptions<DndGameDbContext> options) : DbContext(options)
{
    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.Property(character => character.Name).HasMaxLength(100).IsRequired();
            entity.Property(character => character.Race).HasConversion<string>();
            entity.Property(character => character.Class).HasConversion<string>();
            entity.Property(character => character.StartingTalents).HasColumnType("text[]");
            entity.Property(character => character.DevelopedTalents).HasColumnType("text[]");
            entity.Ignore(character => character.HealthPoints);
            entity.Ignore(character => character.AttackDamage);
            entity.Ignore(character => character.DodgeChance);
            entity.Ignore(character => character.ManaDamage);
            entity.Ignore(character => character.DiceChangeChance);
        });
    }
}
