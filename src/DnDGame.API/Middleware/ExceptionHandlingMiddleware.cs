using System.Text.Json;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Common;

namespace DnDGame.API.Middleware;

/// <summary>
/// Global error-handling middleware (Task 3.19). Sits at the very start of the
/// pipeline (see Program.cs) so it can catch anything thrown by any later
/// middleware or controller.
///
/// Two cases only:
///   1) DomainException  -> an expected, business-level failure. Mapped to an HTTP
///      status via IErrorCodeHttpMapper and returned as ApiErrorResponse, using the
///      exception's own (developer-authored, safe-to-show) Message.
///   2) Anything else     -> an unexpected/internal failure. Logged in full
///      server-side, but the client only ever sees a generic message and
///      ErrorCodes.InternalError — never the exception's message, type, or stack
///      trace.
///
/// This is the only place in the API that catches exceptions this broadly;
/// Controllers and Services are not expected to wrap their own try/catch blocks
/// around business logic — they let DomainException (and genuine bugs) propagate
/// up to here.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IErrorCodeHttpMapper _errorCodeHttpMapper;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IErrorCodeHttpMapper errorCodeHttpMapper)
    {
        _next = next;
        _logger = logger;
        _errorCodeHttpMapper = errorCodeHttpMapper;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException domainException)
        {
            _logger.LogWarning(
                domainException,
                "Request failed with domain error {ErrorCode}: {Message}",
                domainException.ErrorCode,
                domainException.Message);

            await WriteErrorResponseAsync(
                context,
                _errorCodeHttpMapper.Map(domainException.ErrorCode),
                ApiErrorResponse.For(domainException.ErrorCode, domainException.Message));
        }
        catch (Exception unexpectedException)
        {
            // Full details go to the server-side log only — never to the response.
            _logger.LogError(unexpectedException, "Unhandled exception while processing request");

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ApiErrorResponse.For(ErrorCodes.InternalError, "An unexpected error occurred."));
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, ApiErrorResponse error)
    {
        if (context.Response.HasStarted)
        {
            // The response body was already partially written — nothing safe to do
            // but let the connection close; overwriting headers at this point would
            // throw a second exception and hide the original one.
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(error, SerializerOptions));
    }
}
