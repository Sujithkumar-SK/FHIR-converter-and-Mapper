using System.ComponentModel.DataAnnotations;
using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.Auth;

public class VerifyRegistrationOtpRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = null!;
    
    [Required(ErrorMessage = "OTP token is required")]
    public string OtpToken { get; set; } = null!;
    
    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }
}