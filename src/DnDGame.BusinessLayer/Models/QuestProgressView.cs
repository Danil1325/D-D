using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Models;

/// <summary>
/// One player's progress on one quest within a game session.
/// </summary>
public sealed record QuestProgressView(
    int QuestId,
    string Code,
    string Title,
    QuestStatus Status,
    int CurrentObjectiveIndex,
    int? CurrentObjectiveId,
    string? CurrentObjectiveDescription,
    IReadOnlyDictionary<int, int> ObjectiveProgress,
    DateTime? StartedAt,
    DateTime? CompletedAt);