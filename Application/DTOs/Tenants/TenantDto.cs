namespace Application.DTOs.Tenants;

public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? CompanyIdentifier { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionEndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}