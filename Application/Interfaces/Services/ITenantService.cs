namespace Application.Interfaces.Services;

public interface ITenantService
{
    /// <summary>
    /// Get tenant id from jwt
    /// </summary>
    int GetTenantId();
    
    /// <summary>
    /// Get user id from jwt
    /// </summary>
    int GetUserId();
    
    /// <summary>
    /// get username from auth user
    /// </summary>
    string GetUsername();
    
    /// <summary>
    /// check if user.role = superAdmin
    /// </summary>
    bool IsSuperAdmin();
}