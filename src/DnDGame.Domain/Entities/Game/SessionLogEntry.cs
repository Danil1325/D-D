using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// One line in a game session's history — a combat round, a dice check result, a
/// reward, etc. Together these form a readable timeline of one playthrough.
/// </summary>
public class SessionLogEntry : BaseEntity
{
    public int GameSessionId { get; set; }
    public GameSession? GameSession { get; set; }

    public SessionLogEntryType EntryType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
