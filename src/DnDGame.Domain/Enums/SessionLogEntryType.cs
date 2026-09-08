namespace DnDGame.Domain.Enums;

/// <summary>
/// What kind of event a single SessionLogEntry records. One log covers both combat
/// and narrative beats, so a full playthrough can be read back as one timeline.
/// </summary>
public enum SessionLogEntryType
{
    Narrative,
    DiceCheckSuccess,
    DiceCheckFailure,
    CombatRound,
    Reward,
    Damage,
    Info
}
