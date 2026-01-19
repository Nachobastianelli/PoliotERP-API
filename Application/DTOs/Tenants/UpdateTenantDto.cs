using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tenants;

public class UpdateTenantDto
{
    [StringLength(255, MinimumLength = 3, ErrorMessage = "Tenant name must be between 3 and 255 characters long.")]
    public string? Name { get; set; } = string.Empty;
    
    [StringLength(100, MinimumLength =  3, ErrorMessage = "Tenant description must be between 3 and 100 characters long.")]
    public string? Subdomain { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "The company ID must be greater than 50 characters long.")]
    public string? CompanyIdentifier { get; set; } = string.Empty;
    
    public bool? IsActive { get; set; }
    
    public DateTime? SubscriptionEndsAt  { get; set; }
}