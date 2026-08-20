using bow.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace bow.Api.Common.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred while processing request {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path
            );
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
        };

        if (statusCode == 400 || statusCode == 404 || statusCode == 409)
        {
            problemDetails.Detail = exception.Message;
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken
        );

        return true;
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Server error"
        };
    }
}