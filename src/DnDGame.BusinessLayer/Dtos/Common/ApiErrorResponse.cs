namespace DnDGame.BusinessLayer.Dtos.Common;

/// <summary>
/// The single shape used for every error response the API returns, as required by
/// Task 3.19. Never carries a stack trace or any other internals — only a stable,
/// machine-readable ErrorCode a frontend can branch on, and a human-readable Message.
///
/// Example: { "success": false, "errorCode": "CARD_NOT_IN_HAND", "message": "Card is
/// not available in the current hand." }
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public string ErrorCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static ApiErrorResponse For(string errorCode, string message) =>
        new() { ErrorCode = errorCode, Message = message };
}
