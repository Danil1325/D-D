using DnDGame.Domain.Entities.Classes;

namespace DnDGame.BusinessLayer.Dtos.Characters;

/// <summary>One selectable class with the flavor data shown on its New Game card.</summary>
public class ClassOptionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Quote { get; init; } = string.Empty;

    /// <summary>The class's primary attribute, serialized as text (e.g. "Strength").</summary>
    public string PrimaryAttribute { get; init; } = string.Empty;

    public int BaseDamageAmount { get; init; }
    public string ImageFrontPath { get; init; } = string.Empty;
    public string ImageBackPath { get; init; } = string.Empty;

    public static ClassOptionDto FromDomain(CharacterClass characterClass) => new()
    {
        Id = characterClass.Id,
        Name = characterClass.Name,
        Description = characterClass.Description,
        Quote = characterClass.Quote,
        PrimaryAttribute = characterClass.PrimaryAttribute.ToString(),
        BaseDamageAmount = characterClass.BaseDamageAmount,
        ImageFrontPath = characterClass.ImageFrontPath,
        ImageBackPath = characterClass.ImageBackPath
    };
}