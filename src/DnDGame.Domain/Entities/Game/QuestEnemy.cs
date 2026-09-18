namespace DnDGame.Domain.Entities.Game;

/// <summary>An encounter participant, with conditional appearance and optional combat.</summary>
public class QuestEnemy
{
    /// <summary>Existing enemy template, or null for a narrative-only participant such as Guild guards.</summary>
    public int? EnemyId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Exact number when specified by the source; null means the number is not prescribed.</summary>
    public int? Count { get; set; }

    public bool IsOptional { get; set; }
    public int? MinimumAshClock { get; set; }
    public Dictionary<string, bool> RequiredFlags { get; set; } = new();
}
