using DnDGame.Domain.Entities.Races;

namespace DnDGame.BusinessLayer.Dtos.Characters;

/// <summary>
/// One flavor trait shown on a race's card (e.g. Orc's "Powerful Muscles").
/// Purely descriptive — mechanical racial effects live in AttributeRanges instead.
/// </summary>
public class RaceTraitDto
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    /// <summary>UI icon key (e.g. "muscle", "eye") the frontend maps to its own icon component.</summary>
    public string IconKey { get; init; } = string.Empty;

    public static RaceTraitDto FromDomain(RaceTrait trait) => new()
    {
        Name = trait.Name,
        Description = trait.Description,
        IconKey = trait.IconKey
    };
}