namespace DndGame.Domain.Entities;

public enum CharacterRace
{
    Elf,
    Human,
    Orc,
    Dwarf
}

public enum CharacterClass
{
    Healer,
    Warrior,
    Magician,
    Bard
}

public sealed class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CharacterRace Race { get; set; }
    public CharacterClass Class { get; set; }
    public int Health { get; set; } = 10;
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    public List<string> StartingTalents { get; set; } = [];
    public List<string> DevelopedTalents { get; set; } = [];
    public int Level { get; set; } = 1;
    public int TalentPoints { get; set; }
    public int ExperiencePoints { get; set; }
    public int HealthPoints => Health;
    public int AttackDamage => Strength;
    public int DodgeChance => Dexterity;
    public int ManaDamage => Intelligence;
    public int DiceChangeChance => Charisma;
}
