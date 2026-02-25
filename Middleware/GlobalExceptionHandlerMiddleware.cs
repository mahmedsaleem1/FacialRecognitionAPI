using System.Net;
using System.Text.Json;
using FacialRecognitionAPI.Models.DTOs.Responses;

namespace FacialRecognitionAPI.Middleware;

/// <summary>
/// Global exception handler middleware for consistent error responses.
/// Catches all unhandled exceptions and returns structured JSON responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            OperationCanceledException => (HttpStatusCode.BadRequest, "The request was cancelled."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        // Log the exception
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled exception ({StatusCode}): {Message}", (int)statusCode, exception.Message);
        }

        var response = ApiResponse.Fail(message);

        // Include stack trace in development
        if (_env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            response.Errors = new List<string> { exception.ToString() };
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
