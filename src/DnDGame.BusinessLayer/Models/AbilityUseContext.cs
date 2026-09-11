using DnDGame.BusinessLayer.Engines.Interfaces;

namespace DnDGame.BusinessLayer.Models;

/// <summary>
/// Mutable resource pool and selected target for one ability/spell use attempt.
/// Resource is reduced only after all eligibility checks pass.
/// </summary>
public sealed class AbilityUseContext
{
    public AbilityUseContext(int availableResource, ICardTarget? target = null)
    {
        if (availableResource < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableResource));
        }

        AvailableResource = availableResource;
        Target = target;
    }

    public int AvailableResource { get; private set; }

    public ICardTarget? Target { get; }

    internal void Consume(int resourceCost)
    {
        AvailableResource -= resourceCost;
    }
}
