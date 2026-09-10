namespace DnDGame.Domain.Engine.EnemyActions;

/// <summary>
/// An action selected for the active enemy turn.
/// </summary>
public sealed record EnemyAction(EnemyActionType Type);
