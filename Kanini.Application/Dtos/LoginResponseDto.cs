namespace Kanini.Application.DTOs.Auth;

public class LoginResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string OrganizationName { get; set; } = null!;
    public Guid? OrganizationId { get; set; }
    public string Message { get; set; } = null!;
}