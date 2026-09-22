namespace DnDGame.Domain.Enums;

/// <summary>
/// Stable race identifiers used by rules that must not depend on mutable
/// reference-data names. Values align with the mock Race seed identifiers.
/// </summary>
public enum RaceType
{
    Human = 1,
    Elf = 2,
    Orc = 3,
    Dwarf = 4
}
