using DnDGame.BusinessLayer.Dtos.Scenarios;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Outcome of POST /api/locations/travel.</summary>
public class TravelToLocationResultDto
{
    public LocationDetailsDto CurrentLocation { get; init; } = new();
    public StorySceneDto CurrentScene { get; init; } = new();
}
