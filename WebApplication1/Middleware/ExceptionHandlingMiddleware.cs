using System.Net;
using System.Text.Json;
using Application.Exceptions;
using Domain.Exceptions;

namespace WebApplication1.Middleware;

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
        context.Response.ContentType = "application/json";
        
        ErrorResponse response = exception switch
        {
            EntityNotFoundException notFoundEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = notFoundEx.Message,
                Details = new
                {
                    entityName = notFoundEx.EntityName,
                    entityId = notFoundEx.EntityId
                }
            },
            
            DuplicateEntityException duplicateEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Message = duplicateEx.Message,
                Details = new
                {
                    entityName = duplicateEx.EntityName,
                    field = duplicateEx.Field,
                    value = duplicateEx.Value
                }
            },
            
            BusinessRuleValidationException businessEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = businessEx.Message,
                Details = new { rule = businessEx.Rule }
            },
            
            ValidationException validationEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = validationEx.Message,
                Details = new { errors = validationEx.Errors }
            },
            
            UnauthorizedException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "Not authorized. you must sign in",
            },
            
            ForbiddenException forbiddenEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = forbiddenEx.Message
            },
            
            SubscriptionExpiredException subscriptionEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.PaymentRequired,  // Payment Required
                Message = subscriptionEx.Message,
                Details = new
                {
                    expiredAt = subscriptionEx.ExpiredAt,
                    tenantName = subscriptionEx.TenantName
                }
            },

            
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occured",
            }
        };

        context.Response.StatusCode = response.StatusCode;

        // Log según el tipo de error
        if (response.StatusCode >= 500)
        {
            _logger.LogError(exception, "Internal server error: {Message}", exception.Message);
        }
        else if (response.StatusCode >= 400)
        {
            _logger.LogWarning("Client error: ({StatusCode}): {Message}", response.StatusCode, exception.Message);
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}