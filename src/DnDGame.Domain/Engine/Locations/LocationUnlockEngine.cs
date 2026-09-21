using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.Domain.Engine.Locations;

/// <summary>
/// Pure location-progression engine. It changes only the supplied context and
/// reports expected gameplay failures through <see cref="EngineResult{T}"/>.
/// </summary>
public sealed class LocationUnlockEngine : ILocationUnlockEngine
{
    private const int GuildRegistrationQuestId = 3;
    private const int FragmentRouteFlexQuestId = 6;
    private const int DarkstormKeepQuestId = 10;
    private const int HeroOverlookFinaleQuestId = 13;

    private readonly ILocationRouteProvider _routeProvider;

    public LocationUnlockEngine(ILocationRouteProvider? routeProvider = null)
    {
        _routeProvider = routeProvider ?? new LocationRouteProvider();
    }

    public EngineResult<IReadOnlyList<LocationId>> GetAvailableLocations(LocationUnlockContext context)
    {
        var validation = ValidateContext(context);
        if (!validation.Success)
            return EngineResult<IReadOnlyList<LocationId>>.Fail(validation.Message, validation.ErrorCode!);

        var available = new List<LocationId>();
        foreach (var locationId in Enum.GetValues<LocationId>())
        {
            if (context.UnlockedLocationIds.Contains(locationId))
                continue;

            if (CanMeetUnlockRequirements(context, locationId, out _))
                available.Add(locationId);
        }

        // The finale revisits an already-unlocked location, so it is deliberately
        // visible here but cannot be passed to UnlockLocation a second time.
        if (IsHeroOverlookFinaleUnlocked(context) && !available.Contains(LocationId.HerosOverlook))
            available.Add(LocationId.HerosOverlook);

        return EngineResult<IReadOnlyList<LocationId>>.Ok(available);
    }

    public EngineResult<bool> CanUnlockLocation(LocationUnlockContext context, LocationId locationId)
    {
        var validation = ValidateContext(context);
        if (!validation.Success)
            return validation;

        if (context.UnlockedLocationIds.Contains(locationId))
        {
            return EngineResult<bool>.Fail(
                $"Location {locationId} is already unlocked.", EngineErrorCodes.LocationAlreadyUnlocked);
        }

        return CanMeetUnlockRequirements(context, locationId, out var reason)
            ? EngineResult<bool>.Ok(true)
            : EngineResult<bool>.Fail(reason, EngineErrorCodes.LocationRequirementNotMet);
    }

    public EngineResult<LocationUnlockResult> UnlockLocation(LocationUnlockContext context, LocationId locationId)
    {
        var canUnlock = CanUnlockLocation(context, locationId);
        if (!canUnlock.Success)
            return EngineResult<LocationUnlockResult>.Fail(canUnlock.Message, canUnlock.ErrorCode!);

        context.UnlockedLocationIds.Add(locationId);
        context.CurrentLocationId = locationId;
        return EngineResult<LocationUnlockResult>.Ok(CreateResult(context, locationId, LocationStatus.Current));
    }

    public EngineResult<LocationUnlockResult> CompleteLocation(LocationUnlockContext context, LocationId locationId)
    {
        var validation = ValidateContext(context);
        if (!validation.Success)
            return EngineResult<LocationUnlockResult>.Fail(validation.Message, validation.ErrorCode!);

        if (!context.UnlockedLocationIds.Contains(locationId))
        {
            return EngineResult<LocationUnlockResult>.Fail(
                $"Location {locationId} must be unlocked before it can be completed.",
                EngineErrorCodes.LocationNotUnlocked);
        }

        if (!context.CompletedLocationIds.Contains(locationId))
            context.CompletedLocationIds.Add(locationId);
        if (context.CurrentLocationId == locationId)
            context.CurrentLocationId = null;

        return EngineResult<LocationUnlockResult>.Ok(CreateResult(context, locationId, LocationStatus.Completed));
    }

    public EngineResult<LocationId?> GetRecommendedNextLocation(LocationUnlockContext context)
    {
        var validation = ValidateContext(context);
        if (!validation.Success)
            return EngineResult<LocationId?>.Fail(validation.Message, validation.ErrorCode!);

        if (IsHeroOverlookFinaleUnlocked(context))
            return EngineResult<LocationId?>.Ok(LocationId.HerosOverlook);

        var availableResult = GetAvailableLocations(context);
        if (!availableResult.Success || availableResult.Data is null)
            return EngineResult<LocationId?>.Fail(availableResult.Message, availableResult.ErrorCode!);

        var route = _routeProvider.GetRecommendedRoute(context.Race);
        var next = route.Steps
            .OrderBy(step => step.Order)
            .Select(step => step.LocationId)
            .FirstOrDefault(locationId => availableResult.Data.Contains(locationId));

        return EngineResult<LocationId?>.Ok(
            availableResult.Data.Contains(next) ? next : null);
    }

    private bool CanMeetUnlockRequirements(LocationUnlockContext context, LocationId locationId, out string reason)
    {
        if (locationId == LocationId.HerosOverlook)
        {
            reason = string.Empty;
            return true;
        }

        var route = _routeProvider.GetRecommendedRoute(context.Race);
        var firstRaceLocation = route.Steps.OrderBy(step => step.Order).Skip(1).First().LocationId;
        var prologueCompleted = context.CompletedLocationIds.Contains(LocationId.HerosOverlook);

        if (locationId == firstRaceLocation)
        {
            reason = "Complete the Hero's Overlook prologue before following the racial route.";
            return prologueCompleted;
        }

        if (locationId == LocationId.MisthavenPort)
        {
            reason = "Complete the racial-route location before travelling to Misthaven Port.";
            return context.CompletedLocationIds.Contains(firstRaceLocation);
        }

        if (locationId == LocationId.Oakheaven)
        {
            reason = "Guild registration (MQ-03) is required before travelling to Oakheaven.";
            return context.CompletedQuestIds.Contains(GuildRegistrationQuestId) ||
                   context.StoryFlags.GetValueOrDefault("GuildRegistered");
        }

        if (locationId == LocationId.DarkstormKeep)
        {
            reason = "Darkstorm Keep requires all three Crown Fragments and completion of MQ-10.";
            return context.CrownFragmentCount >= 3 &&
                   context.CompletedQuestIds.Contains(DarkstormKeepQuestId);
        }

        if (context.CompletedQuestIds.Contains(FragmentRouteFlexQuestId) &&
            route.FlexibleFragmentLocationIds.Contains(locationId))
        {
            reason = string.Empty;
            return true;
        }

        var step = route.Steps
            .OrderBy(candidate => candidate.Order)
            .FirstOrDefault(candidate => candidate.LocationId == locationId);
        if (step is null || step.Order <= 1)
        {
            reason = $"Location {locationId} is not available on the configured route.";
            return false;
        }

        var previousLocation = route.Steps.Single(candidate => candidate.Order == step.Order - 1).LocationId;
        reason = $"Complete {previousLocation} before travelling to {locationId}.";
        return context.CompletedLocationIds.Contains(previousLocation);
    }

    private static EngineResult<bool> ValidateContext(LocationUnlockContext? context)
    {
        if (context is null)
        {
            return EngineResult<bool>.Fail(
                "Location unlock context is required.", EngineErrorCodes.LocationInvalidContext);
        }

        if (context.PlayerId <= 0 || context.Level <= 0 || !Enum.IsDefined(context.Race))
        {
            return EngineResult<bool>.Fail(
                "Location unlock context contains an invalid player, level, or race.",
                EngineErrorCodes.LocationInvalidContext);
        }

        return EngineResult<bool>.Ok(true);
    }

    private LocationUnlockResult CreateResult(
        LocationUnlockContext context, LocationId locationId, LocationStatus status)
    {
        var available = GetAvailableLocations(context);
        var next = GetRecommendedNextLocation(context);
        return new LocationUnlockResult
        {
            PlayerId = context.PlayerId,
            LocationId = locationId,
            Status = status,
            HeroOverlookFinaleUnlocked = IsHeroOverlookFinaleUnlocked(context),
            AvailableLocationIds = available.Data ?? Array.Empty<LocationId>(),
            RecommendedNextLocationId = next.Data
        };
    }

    private static bool IsHeroOverlookFinaleUnlocked(LocationUnlockContext context) =>
        context.CompletedQuestIds.Contains(HeroOverlookFinaleQuestId);
}
