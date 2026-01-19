using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Application.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "The Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "The Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "The FirstName is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "The FirstName must be between 3 and 50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "The LastName is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "The LastName must be between 3 and 50 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "The Email is required")]
    [StringLength(320, MinimumLength = 10, ErrorMessage = "The  Email must be between 10 and 320 characters")]
    [EmailAddress(ErrorMessage = "The Email address is not valid")]
    public string Email { get; set; } = string.Empty;
    
    
    [Required(ErrorMessage = "The Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "The PhoneNumber is required")]
    [StringLength(25, MinimumLength = 10, ErrorMessage = "The PhoneNumber must be between 10 and 25 characters")]
    public string PhoneNumber { get; set; } = string.Empty;
    
}