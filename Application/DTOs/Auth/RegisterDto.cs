using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterDto
{
    //Tenant Data 
    [Required(ErrorMessage = "TenantName is required")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "TenantName must be between 3 and 255 characters long")]
    public string TenantName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Subdomain is required")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Subdomain must be alphanumeric  only")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Subdomain must be between 3 and 100 characters long")]
    public string Subdomain { get; set; } = string.Empty;
    [StringLength(50,ErrorMessage = "CompanyIdentifier must be less than 50 characters long")]
    public string? CompanyIdentifier { get; set; }
    
    //User Data Admin UDA (Tenant Owner)
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters long")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "FirstName must be between 3 and 50 characters long")]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "LastName is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "LastName must be between 3 and 50 characters long")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "PhoneNumber is required")]
    [Phone(ErrorMessage = "PhoneNumber must be a valid phone number")]
    [StringLength(25 , MinimumLength = 10, ErrorMessage = "PhoneNumber must be between 10 and 25 characters long")]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(320, ErrorMessage = "Email must be less than 320 characters long")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; }  = string.Empty;
    
    
}