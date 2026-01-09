namespace Kanini.Application.DTOs.Auth;

public class RegistrationResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Message { get; set; } = null!;
}