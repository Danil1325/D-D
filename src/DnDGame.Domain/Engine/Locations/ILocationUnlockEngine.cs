using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.Domain.Engine.Locations;

public interface ILocationUnlockEngine
{
    EngineResult<IReadOnlyList<LocationId>> GetAvailableLocations(LocationUnlockContext context);
    EngineResult<bool> CanUnlockLocation(LocationUnlockContext context, LocationId locationId);
    EngineResult<LocationUnlockResult> UnlockLocation(LocationUnlockContext context, LocationId locationId);
    EngineResult<LocationUnlockResult> CompleteLocation(LocationUnlockContext context, LocationId locationId);
    EngineResult<LocationId?> GetRecommendedNextLocation(LocationUnlockContext context);
}
