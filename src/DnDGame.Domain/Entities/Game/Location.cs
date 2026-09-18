using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

public class Location : BaseEntity
{
    /// <summary>Stable route identifier, separate from the numeric ID used by scenes and quests.</summary>
    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecommendedMinimumLevel { get; set; } = 1;

    /// <summary>Exact frontend image key, without a file extension or path.</summary>
    public string BackgroundImage { get; set; } = string.Empty;

    /// <summary>IDs of main quests offered here, subject to their prerequisites.</summary>
    public ICollection<int> AvailableMainQuestIds { get; set; } = new List<int>();

    /// <summary>IDs of side quests offered here, subject to their prerequisites.</summary>
    public ICollection<int> AvailableSideQuestIds { get; set; } = new List<int>();

    /// <summary>Enemy families that can be encountered at this location.</summary>
    public ICollection<EnemyFamily> PossibleEnemyTypes { get; set; } = new List<EnemyFamily>();

    public bool IsSafeLocation { get; set; }
}
