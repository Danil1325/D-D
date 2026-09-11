namespace DnDGame.BusinessLayer.Models;

using DnDGame.Domain.Enums;

/// <summary>
/// Generic result object for engine operations.
/// Encapsulates success/failure status along with optional error codes, messages, and data.
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public class EngineResult<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// The error code if the operation failed. Null if successful.
    /// </summary>
    public ErrorCode? ErrorCode { get; private set; }

    /// <summary>
    /// A descriptive error message. Null if successful.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// The data returned on success. Default/null if operation failed.
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// Private constructor to enforce use of static factory methods.
    /// </summary>
    private EngineResult(bool isSuccess, T? data, ErrorCode? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful result with the specified data.
    /// </summary>
    /// <param name="data">The data to return.</param>
    /// <returns>A successful EngineResult containing the data.</returns>
    public static EngineResult<T> Success(T? data = default)
    {
        return new EngineResult<T>(true, data, null, null);
    }

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code indicating the type of failure.</param>
    /// <param name="errorMessage">A descriptive error message.</param>
    /// <returns>A failed EngineResult with error information.</returns>
    public static EngineResult<T> Failure(ErrorCode errorCode, string errorMessage)
    {
        return new EngineResult<T>(false, default, errorCode, errorMessage);
    }
}

/// <summary>
/// Non-generic variant of EngineResult for operations that don't return data.
/// </summary>
public class EngineResult
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// The error code if the operation failed. Null if successful.
    /// </summary>
    public ErrorCode? ErrorCode { get; private set; }

    /// <summary>
    /// A descriptive error message. Null if successful.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Private constructor to enforce use of static factory methods.
    /// </summary>
    private EngineResult(bool isSuccess, ErrorCode? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful EngineResult.</returns>
    public static EngineResult Success()
    {
        return new EngineResult(true, null, null);
    }

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code indicating the type of failure.</param>
    /// <param name="errorMessage">A descriptive error message.</param>
    /// <returns>A failed EngineResult with error information.</returns>
    public static EngineResult Failure(ErrorCode errorCode, string errorMessage)
    {
        return new EngineResult(false, errorCode, errorMessage);
    }
}
