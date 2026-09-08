namespace DnDGame.Domain.Enums;

/// <summary>
/// The five character attributes used throughout the game. This is the simplified
/// replacement for D&D's six ability scores — Constitution and Wisdom are NOT used.
///
/// Health doubles as the character's hit-point pool (MaxHealth/CurrentHealth). The
/// other four are added directly to 2d6 rolls in checks and combat — there is no
/// ability-modifier conversion table anywhere in this game.
/// </summary>
public enum AttributeType
{
    Health,
    Strength,
    Dexterity,
    Intelligence,
    Charisma
}
