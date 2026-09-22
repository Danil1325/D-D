using DnDGame.BusinessLayer.Dtos.Scenarios;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>
/// Response shape after a successful location travel transition.
/// </summary>
public class TravelToLocationResultDto
{
    public LocationDetailsDto CurrentLocation { get; init; } = new();
    public StorySceneDto CurrentScene { get; init; } = new();
}
