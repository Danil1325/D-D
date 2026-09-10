namespace DnDGame.API.Middleware;

public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>Registers the global error-handling middleware. Call this first in the pipeline, before UseHttpsRedirection/UseAuthorization/MapControllers.</summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
