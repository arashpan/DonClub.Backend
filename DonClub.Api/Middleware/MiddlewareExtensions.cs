using Microsoft.AspNetCore.Builder;

namespace Donclub.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
