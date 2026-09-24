using DnDGame.Domain.Entities.Collection;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Collection;

/// <summary>One Collection catalog entry, independent of any player.</summary>
public class CollectionEntryDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public CollectionCategory Category { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public CollectionRarity? Rarity { get; init; }
    public IReadOnlyDictionary<string, string>? Stats { get; init; }
    public string? Lore { get; init; }

    public static CollectionEntryDto FromDomain(CollectionEntry entry) => new()
    {
        Id = entry.Id,
        Code = entry.Code,
        Category = entry.Category,
        Name = entry.Name,
        Description = entry.Description,
        Rarity = entry.Rarity,
        Stats = entry.Stats,
        Lore = entry.Lore
    };
}
