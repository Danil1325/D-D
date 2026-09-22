namespace DnDGame.Domain.Entities.Game;

/// <summary>Reward data awarded upon quest completion; applying rewards belongs to game logic.</summary>
public class QuestReward
{
    /// <summary>Experience awarded to PlayerCharacter.CurrentXp.</summary>
    public int Experience { get; set; }

    /// <summary>Item quantities to grant, keyed by item identifier.</summary>
    public Dictionary<int, int> Items { get; set; } = new();

    /// <summary>Story flag values to assign, including false to clear a flag.</summary>
    public Dictionary<string, bool> StoryFlags { get; set; } = new();

    /// <summary>Signed loyalty changes keyed by companion identifier.</summary>
    public Dictionary<int, int> CompanionLoyalty { get; set; } = new();

    /// <summary>Signed change to the scenario's war score.</summary>
    public int WarScore { get; set; }

    /// <summary>Explicit location unlock ids authored on this reward.</summary>
    public ICollection<int> NewLocationIds { get; set; } = new List<int>();
}
