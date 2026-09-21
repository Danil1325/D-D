namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Minimal enemy data displayed by the map UI.</summary>
public class LocationEnemyDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
