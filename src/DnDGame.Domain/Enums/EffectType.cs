namespace DnDGame.Domain.Enums;

/// <summary>
/// Defines the type of effect a card can produce.
/// </summary>
public enum EffectType
{
    Damage = 0,
    Healing = 1,
    BuffAttribute = 2,
    DebuffAttribute = 3,
    StatusEffect = 4,
    Draw = 5,
    Discard = 6,
    Stun = 7,
    Protect = 8,
    Custom = 9,
    Strength = 10,
    Weak = 11,
    Vulnerable = 12,
    DefenseUp = 13
}
