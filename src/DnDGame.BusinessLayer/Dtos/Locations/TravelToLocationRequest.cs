using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Dtos.Locations;

public class TravelToLocationRequest
{
    public int PlayerId { get; init; }
    public LocationId LocationId { get; init; }
}
