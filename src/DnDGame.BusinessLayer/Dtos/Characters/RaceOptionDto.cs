using DnDGame.Domain.Entities.Races;

namespace DnDGame.BusinessLayer.Dtos.Characters;

/// <summary>One selectable race with the flavor data shown on its New Game card.</summary>
public class RaceOptionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Quote { get; init; } = string.Empty;
    public string ImageFrontPath { get; init; } = string.Empty;
    public string ImageBackPath { get; init; } = string.Empty;

    public IReadOnlyList<RaceTraitDto> Traits { get; init; } = Array.Empty<RaceTraitDto>();

    /// <summary>The min/max roll range for each of the 5 attributes of this race.</summary>
    public IReadOnlyList<AttributeRangeDto> AttributeRanges { get; init; } = Array.Empty<AttributeRangeDto>();

    public static RaceOptionDto FromDomain(Race race) => new()
    {
        Id = race.Id,
        Name = race.Name,
        Description = race.Description,
        Quote = race.Quote,
        ImageFrontPath = race.ImageFrontPath,
        ImageBackPath = race.ImageBackPath,
        Traits = race.Traits.Select(RaceTraitDto.FromDomain).ToList(),
        AttributeRanges = race.AttributeRanges.Select(AttributeRangeDto.FromDomain).ToList()
    };
}