namespace Kanini.Application.Services.Users;

public interface IOTPService
{
    string GenerateOTP();
    string CreateOtpToken(string email, string otp, string registrationData);
    bool ValidateOtpToken(string otpToken, string otp, string email);
}