using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var detail = string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, new ErrorResponse
            {
                Type = "ValidationError",
                Error = "Validation Failed",
                Detail = detail
            });
        }
        catch (DomainException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, new ErrorResponse
            {
                Type = "BusinessRuleViolation",
                Error = ex.Message,
                Detail = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, new ErrorResponse
            {
                Type = "ResourceNotFound",
                Error = ex.Message,
                Detail = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, new ErrorResponse
            {
                Type = "AuthenticationError",
                Error = "Unauthorized",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Type = "InternalServerError",
                Error = "An unexpected error occurred",
                Detail = _env.IsDevelopment() ? ex.ToString() : string.Empty
            });
        }
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, ErrorResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
