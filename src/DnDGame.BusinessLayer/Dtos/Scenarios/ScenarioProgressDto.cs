using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Dtos.Scenarios;

/// <summary>
/// Response shape for the scenario run's persistent state: POST
/// /api/scenario/start/{playerId} and POST /api/scenario/choice. Mirrors the
/// domain <see cref="ScenarioProgress"/> entity without exposing it.
/// </summary>
public class ScenarioProgressDto
{
    public int GameSessionId { get; init; }
    public int CurrentSceneId { get; init; }
    public bool IsCompleted { get; init; }
    public int AshClock { get; init; }
    public int Corruption { get; init; }
    public int WarScore { get; init; }

    public IReadOnlyDictionary<int, int> CompanionLoyalty { get; init; } = new Dictionary<int, int>();
    public IReadOnlyDictionary<int, int> QuestProgress { get; init; } = new Dictionary<int, int>();
    public IReadOnlyDictionary<string, bool> StoryFlags { get; init; } = new Dictionary<string, bool>();
    public IReadOnlyCollection<int> NewLocationIds { get; init; } = Array.Empty<int>();

    public static ScenarioProgressDto FromDomain(
        ScenarioProgress progress,
        IEnumerable<int>? newLocationIds = null) => new()
    {
        GameSessionId = progress.GameSessionId,
        CurrentSceneId = progress.CurrentSceneId,
        IsCompleted = progress.IsCompleted,
        AshClock = progress.AshClock,
        Corruption = progress.Corruption,
        WarScore = progress.WarScore,
        CompanionLoyalty = new Dictionary<int, int>(progress.CompanionLoyalty),
        QuestProgress = new Dictionary<int, int>(progress.QuestProgress),
        StoryFlags = new Dictionary<string, bool>(progress.StoryFlags),
        NewLocationIds = newLocationIds is null ? Array.Empty<int>() : newLocationIds.Distinct().ToList()
    };
}
