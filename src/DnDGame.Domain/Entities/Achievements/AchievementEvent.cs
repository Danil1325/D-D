using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Achievements;

/// <summary>
/// One real, already-counted game event, ledgered per player so an achievement can
/// never be granted twice for the same event even if the backend hook fires twice
/// (e.g. the same battle victory being re-evaluated, or the same quest being
/// finalized through two paths). Composite-keyed (PlayerId, Type, EventKey) — the
/// same shape LocationProgress uses for other per-player rows.
///
/// EventKey is the stable identity of the underlying game record (battle id, quest
/// id, location id, ...) as text; Type says which event family it belongs to. Each
/// event family advances exactly the achievements whose AchievementType matches
/// that Type.
/// </summary>
public class AchievementEvent
{
    public int PlayerId { get; set; }
    public AchievementType Type { get; set; }

    /// <summary>Stable identity of the underlying game record, e.g. "17" (battle id).</summary>
    public string EventKey { get; set; } = string.Empty;

    /// <summary>When the event was first recorded for this player.</summary>
    public DateTime RecordedAt { get; set; }
}