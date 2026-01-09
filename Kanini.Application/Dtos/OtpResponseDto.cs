namespace Kanini.Application.DTOs.Auth;

public class OtpResponseDto
{
    public string Email { get; set; } = null!;
    public string OtpToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string Message { get; set; } = null!;
}