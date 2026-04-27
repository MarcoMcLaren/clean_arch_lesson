using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Presentation.Middleware;

// ExceptionHandlingMiddleware catches ALL unhandled exceptions from the pipeline.
// It maps Domain exceptions to HTTP status codes here in Presentation —
// the Domain layer knows nothing about HTTP, which is the correct separation.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            BicycleNotFoundException        => (StatusCodes.Status404NotFound,            "Resource Not Found"),
            BicycleNotAvailableException    => (StatusCodes.Status409Conflict,             "Bicycle Not Available"),
            InvalidRentalOperationException => (StatusCodes.Status400BadRequest,           "Invalid Operation"),
            _                               => (StatusCodes.Status500InternalServerError,  "Internal Server Error")
        };

        if (statusCode == 500)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Domain exception [{Type}]: {Message}", exception.GetType().Name, exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
