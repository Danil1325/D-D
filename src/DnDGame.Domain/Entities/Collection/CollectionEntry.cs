using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Collection;

/// <summary>
/// Reference data — one browsable entry on the Collection screen (a hero, class,
/// enemy, weapon, armor piece or potion), seeded once at startup like Achievement
/// and never created or edited by players.
///
/// <see cref="Code"/> is the stable identifier the frontend already uses for the
/// same entry (e.g. "iron-sword", "the-lich"), kept so both sides stay keyed the
/// same way.
///
/// There is deliberately no per-player "discovered" state here: the frontend's
/// own collectionData has no real game event (a battle win, a purchase, ...)
/// that ever flips it, so this catalog is read-only and always fully visible.
/// See CollectionService's remarks.
/// </summary>
public class CollectionEntry
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public CollectionCategory Category { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public CollectionRarity? Rarity { get; set; }

    /// <summary>Arbitrary flavor stats (e.g. HP, Damage, Speed), stored as text to match the frontend's mixed string/number values.</summary>
    public IReadOnlyDictionary<string, string>? Stats { get; set; }

    public string? Lore { get; set; }
}
