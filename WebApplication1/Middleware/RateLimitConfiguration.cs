using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace WebApplication1.Middlewares;

public static class RateLimitConfiguration
{
    public static void ConfigureRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Login Rate limiter (per IP)
            options.AddFixedWindowLimiter("auth", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 5; // 5 request per minute
                opt.QueueLimit = 0;
            });

            // Register Rate limiter (per IP)
            options.AddFixedWindowLimiter("register", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(10);
                opt.PermitLimit = 3; // 3 request per 10 minutes
                opt.QueueLimit = 0;
            });
            
            // Rate limit for critical endpoints (per IP)
            options.AddFixedWindowLimiter("critical", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 10; // 10 request per minute
                opt.QueueLimit = 0;
            });

            // Global Rate limit API (per authenticate user or IP)
            options.AddFixedWindowLimiter("api", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 100;
                opt.QueueLimit = 0;
            });
            
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429; // Too Many Requests
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfterSeconds = context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter, 
                    out var retryAfter) 
                        ? (int)retryAfter.TotalSeconds 
                        : 60;

                var response = new
                {
                    message = "Too many requests. Please try again later.",
                    retryAfter = retryAfterSeconds
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };
        });
    }
}