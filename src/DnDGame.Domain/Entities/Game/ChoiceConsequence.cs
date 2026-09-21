namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// Describes changes caused by a choice. Numeric values are signed deltas (zero is no change).
/// Dictionary entries identify affected companions, quests and flags.
/// Applying these changes belongs to the scenario logic.
/// </summary>
public class ChoiceConsequence
{
    /// <summary>Delta applied to PlayerCharacter.CurrentXp.</summary>
    public int EXP { get; set; }

    public int AshClock { get; set; }
    public int Corruption { get; set; }
    public int WarScore { get; set; }

    /// <summary>Loyalty deltas keyed by companion identifier.</summary>
    public Dictionary<int, int> CompanionLoyalty { get; set; } = new();

    /// <summary>Progress deltas keyed by quest identifier. An absent quest starts at zero.</summary>
    public Dictionary<int, int> QuestProgress { get; set; } = new();

    /// <summary>Flag values to assign, including false to clear a flag.</summary>
    public Dictionary<string, bool> StoryFlags { get; set; } = new();

    /// <summary>Explicit location unlock ids authored on this consequence.</summary>
    public ICollection<int> NewLocationIds { get; set; } = new List<int>();
}
