using DnDGame.BusinessLayer.Models;

namespace DnDGame.BusinessLayer.Dtos.Decks;

/// <summary>Translates BusinessLayer.Models.EngineResult (Domain.Enums.ErrorCode) into an API-friendly shape.</summary>
public class DeckValidationResultDto
{
    public bool IsValid { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static DeckValidationResultDto FromEngineResult(EngineResult result) => new()
    {
        IsValid = result.IsSuccess,
        ErrorCode = result.ErrorCode?.ToString(),
        ErrorMessage = result.ErrorMessage
    };
}
