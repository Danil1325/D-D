using DnDGame.Domain.Entities.Enemies;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Minimal enemy data displayed by the map UI.</summary>
public class LocationEnemyDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public static LocationEnemyDto FromDomain(Enemy enemy) => new()
    {
        Id = enemy.Id,
        Name = enemy.Name
    };
}
