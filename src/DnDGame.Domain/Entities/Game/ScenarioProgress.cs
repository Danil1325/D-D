using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// Interactive scenario state for one GameSession, which identifies the character and adventure.
/// Experience, race, class and level remain on the session's PlayerCharacter.
/// Numeric values here are current totals, rather than consequence deltas.
/// </summary>
public class ScenarioProgress : BaseEntity
{
    public int GameSessionId { get; set; }
    public int CurrentSceneId { get; set; }
    public bool IsCompleted { get; set; }
    public int AshClock { get; set; }
    public int Corruption { get; set; }
    public int WarScore { get; set; }
    public Dictionary<int, int> CompanionLoyalty { get; set; } = new();
    public Dictionary<int, int> QuestProgress { get; set; } = new();
    public Dictionary<string, bool> StoryFlags { get; set; } = new();
    public HashSet<int> UnlockedLocationIds { get; set; } = new();
}
