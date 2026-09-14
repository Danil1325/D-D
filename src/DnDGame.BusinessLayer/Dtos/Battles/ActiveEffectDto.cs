using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.BusinessLayer.Dtos.Battles;

/// <summary>Response shape for one buff/debuff currently active on a combatant.</summary>
public class ActiveEffectDto
{
    public Guid EffectInstanceId { get; init; }
    public string Type { get; init; } = string.Empty;
    public int Value { get; init; }
    public int Duration { get; init; }
    public int StackCount { get; init; }
    public EffectTarget Target { get; init; }

    public static ActiveEffectDto FromDomain(ActiveEffect effect) => new()
    {
        EffectInstanceId = effect.EffectInstanceId,
        Type = effect.Type,
        Value = effect.Value,
        Duration = effect.Duration,
        StackCount = effect.StackCount,
        Target = effect.Target
    };
}
