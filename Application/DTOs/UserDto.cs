namespace Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; }  = string.Empty; 
    public string FirstName { get; set; }  = string.Empty;
    public string LastName { get; set; }   = string.Empty;
    public string PhoneNumber { get; set; }  = string.Empty;
    public bool IsActive { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}