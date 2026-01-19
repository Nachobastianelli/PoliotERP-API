using Domain.Common;

namespace Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? CompanyIdentifier { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionEndsAt { get; set; }
    
    //Representa la relacion 1-muchos de tenant a Users
    public ICollection<User> Users { get; set; } = new List<User>();
}