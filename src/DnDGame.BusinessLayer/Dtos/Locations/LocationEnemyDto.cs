using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>
/// One enemy encounter currently offered to the player at a location. Only ever
/// built from ILocationEncounterService's already-filtered result, so the frontend
/// never receives an encounter the player cannot actually fight yet.
/// </summary>
public class LocationEnemyDto
{
    public int EnemyId { get; init; }
    public string EnemyName { get; init; } = string.Empty;
    public EncounterTier Tier { get; init; }
}
