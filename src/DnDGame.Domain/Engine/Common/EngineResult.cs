namespace DnDGame.Domain.Engine.Common;

/// <summary>
/// Represents the outcome of a game-engine operation without using exceptions for
/// expected gameplay failures.
/// </summary>
/// <typeparam name="T">The type returned when the operation succeeds.</typeparam>
public sealed class EngineResult<T>
{
    private EngineResult(bool success, string message, string? errorCode, T? data)
    {
        Success = success;
        Message = message;
        ErrorCode = errorCode;
        Data = data;
    }

    public bool Success { get; }

    public string Message { get; }

    public string? ErrorCode { get; }

    public T? Data { get; }

    /// <summary>
    /// Creates a successful result containing operation data.
    /// </summary>
    public static EngineResult<T> Ok(T data, string message = "")
    {
        return new EngineResult<T>(true, message, null, data);
    }

    /// <summary>
    /// Creates a failed result for an expected game-engine condition.
    /// </summary>
    public static EngineResult<T> Fail(string message, string errorCode)
    {
        return new EngineResult<T>(false, message, errorCode, default);
    }
}
