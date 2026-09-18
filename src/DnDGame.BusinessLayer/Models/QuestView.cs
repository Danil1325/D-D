using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Models;

/// <summary>
/// Public description of a quest from the catalog, as a service returns it.
/// </summary>
public sealed record QuestView(
    int Id,
    string Code,
    string Title,
    string Description,
    QuestType QuestType,
    string QuestGiver,
    int RecommendedLevel,
    int? RecommendedMaximumLevel,
    int? LocationId,
    IReadOnlyCollection<int> PossibleLocationIds,
    bool IsOptional,
    int ExperienceReward);