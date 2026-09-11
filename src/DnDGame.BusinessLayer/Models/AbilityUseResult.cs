using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Models;

/// <summary>Details of a successfully validated and paid ability/spell use.</summary>
public sealed record AbilityUseResult(
    int ResourceCost,
    int RemainingResource,
    TargetType TargetType,
    EffectType EffectType);
