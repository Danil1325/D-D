using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Models;

/// <summary>
/// One enemy the player could currently encounter. Deliberately minimal — just
/// enough for a caller to display a choice and start a battle by EnemyId; the full
/// Enemy stat block stays in IEnemyRepository rather than being duplicated here.
/// </summary>
public sealed record EncounterOption(int EnemyId, string EnemyName, EncounterTier Tier);
