using Application.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetTenantId()
    {
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("tenantId")?.Value;
        if (String.IsNullOrEmpty(tenantIdClaim))
            throw new UnauthorizedException("TenantID not found in a token");
        
        return int.Parse(tenantIdClaim);
    }

    public int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
        
        if (String.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedException("UserId not found in a token");
        
        return int.Parse(userIdClaim);
    }

    public string GetUsername()
    {
        var usernameClaim = _httpContextAccessor.HttpContext?.User.FindFirst("username")?.Value;
        return usernameClaim ?? throw new UnauthorizedException("Username not found in a token");
    }

    public bool IsSuperAdmin()
    {
        var roles = _httpContextAccessor.HttpContext?.User.FindAll("role").Select(c => c.Value).ToList() ?? new List<string>();
        return roles.Contains("SuperAdmin");
    }
}