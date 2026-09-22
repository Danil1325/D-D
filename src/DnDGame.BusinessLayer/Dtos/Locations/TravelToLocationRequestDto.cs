namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>
/// Request shape for POST /api/locations/travel.
/// </summary>
public class TravelToLocationRequestDto
{
    public int PlayerId { get; set; }
    public int LocationId { get; set; }
}
