using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Locations;

/// <summary>
/// The full enemy-encounter pool configured for one location: which enemies can be
/// encountered there, at which EncounterTier, and under what availability conditions.
/// Keyed to the shared LocationId enum, consistent with LocationRouteStep and the
/// rest of this namespace's location-progression models.
/// </summary>
public class LocationEncounterDefinition : BaseEntity
{
    public LocationId LocationId { get; set; }
    public ICollection<EncounterEnemyDefinition> Enemies { get; set; } = new List<EncounterEnemyDefinition>();
}
