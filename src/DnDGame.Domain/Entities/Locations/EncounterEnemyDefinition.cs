using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Locations;

/// <summary>One enemy slot within a LocationEncounterDefinition's pool.</summary>
public class EncounterEnemyDefinition
{
    /// <summary>References an Enemy row (Domain.Entities.Enemies.Enemy).</summary>
    public int EnemyId { get; set; }

    public EncounterTier Tier { get; set; }

    /// <summary>Null means always available (subject only to the location itself being reachable).</summary>
    public EncounterAvailabilityRequirement? Availability { get; set; }
}
