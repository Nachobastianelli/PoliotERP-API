using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace WebApplication1.Middleware;

public class SubscriptionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionValidationMiddleware> _logger;

    public SubscriptionValidationMiddleware(
        RequestDelegate next,
        ILogger<SubscriptionValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantRepository tenantRepository, 
        ITenantService tenantService)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/auth/login") ||
            path.Contains("/auth/register") ||
            path.Contains("/scalar") ||
            path.Contains("/openapi"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                if (tenantService.IsSuperAdmin())
                {
                    await _next(context);
                    return;
                }

                var tenantId = tenantService.GetTenantId();
                var tenant = await tenantRepository.GetByIdAsync(tenantId);

                if (tenant == null)
                {
                    _logger.LogWarning("Tenant {tenantId} not found for authenticated user", tenantId);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Organization not found"
                    });
                    return;
                }

                if (tenant.SubscriptionEndsAt.HasValue && tenant.SubscriptionEndsAt.Value < DateTime.UtcNow)
                {
                    if (tenant.IsActive)
                    { 
                        _logger.LogWarning("Subscription expired for tenant {tenantId} ({tenant.Name}). Deactivating.",tenant, tenant.Name);
                        tenant.IsActive = false;
                        tenant.UpdatedAt = DateTime.UtcNow;
                        await tenantRepository.UpdateAsync(tenant);
                    }
                    
                    context.Response.StatusCode = 402; //Payment required
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Subscription expired. Please renew to continue.",
                        SubscriptionEndsAt = tenant.SubscriptionEndsAt.Value,
                    });
                    throw new SubscriptionExpiredException(tenant.SubscriptionEndsAt.Value, tenant.Name);
                }

                if (!tenant.IsActive)
                {
                    _logger.LogWarning("Tenant {tenantId} ({tenant.Name}) is inactive", tenantId, tenant.Name);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Organization is inactive. Please contact support.",
                        }
                    );
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                
            }
        }
        await _next(context);
    }
    
}