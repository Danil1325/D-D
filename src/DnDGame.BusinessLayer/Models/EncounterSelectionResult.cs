using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Models;

/// <summary>Result of filtering a location's encounter pool for the current player/session state.</summary>
public sealed record EncounterSelectionResult(
    LocationId LocationId,
    IReadOnlyList<EncounterOption> AvailableEncounters);
