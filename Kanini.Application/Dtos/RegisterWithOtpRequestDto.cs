using System.ComponentModel.DataAnnotations;
using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.Auth;

public class RegisterWithOtpRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }

    [Required(ErrorMessage = "Organization name is required")]
    [MaxLength(200, ErrorMessage = "Organization name cannot exceed 200 characters")]
    public string OrganizationName { get; set; } = null!;

    [Required(ErrorMessage = "Mobile number is required")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number format")]
    public string MobileNumber { get; set; } = null!;
}