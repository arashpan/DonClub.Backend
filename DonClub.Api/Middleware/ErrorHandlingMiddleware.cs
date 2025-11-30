using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Donclub.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // لاگ کامل برای خودمون
        _logger.LogError(ex, "Unhandled exception caught by ErrorHandlingMiddleware.");

        var statusCode = HttpStatusCode.InternalServerError;
        string errorCode = "SERVER_ERROR";
        string message = "An unexpected error occurred.";

        switch (ex)
        {
            case InvalidOperationException ioe:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "INVALID_OPERATION";
                message = ioe.Message;
                break;

            case KeyNotFoundException knf:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                message = knf.Message;
                break;

                // در صورت نیاز Exceptionهای خاص دیگر هم اضافه کن
        }

        var problem = new
        {
            success = false,
            error = new
            {
                code = errorCode,
                message,
                status = (int)statusCode,
                traceId = context.TraceIdentifier
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}
