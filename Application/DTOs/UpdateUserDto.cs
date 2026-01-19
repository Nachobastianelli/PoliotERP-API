using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "The first name must be between 3 and 50 characters")]
    public string? FirstName { get; set; }
    
    [StringLength(50, MinimumLength = 3, ErrorMessage = "The last name must be between 3 and 50 characters")]
    public string? LastName { get; set; }
    
    [StringLength(50, MinimumLength = 3, ErrorMessage = "The username must be between 3 and 50 characters")]
    public string? Username { get; set; }
    
    [StringLength(320, MinimumLength = 10, ErrorMessage = "The  Email must be between 10 and 320 characters")]
    [EmailAddress(ErrorMessage = "The Email address is not valid")]
    public string? Email { get; set; }
    
    [StringLength(25, MinimumLength = 10, ErrorMessage = "The PhoneNumber must be between 10 and 25 characters")]
    public string? PhoneNumber { get; set; }
    
    public bool? IsActive { get; set; }
    
    
}