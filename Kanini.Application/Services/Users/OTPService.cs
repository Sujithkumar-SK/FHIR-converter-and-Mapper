using Kanini.Application.Services.Users;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Kanini.Application.Services.Users;

public class OTPService : IOTPService
{
    private readonly ILogger<OTPService> _logger;

    public OTPService(ILogger<OTPService> logger)
    {
        _logger = logger;
    }

    public string GenerateOTP()
    {
        try
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OTP");
            throw;
        }
    }

    public string CreateOtpToken(string email, string otp, string registrationData)
    {
        try
        {
            var tokenData = $"{email}|{otp}|{DateTime.UtcNow.AddMinutes(5):yyyy-MM-dd HH:mm:ss}|{registrationData}";
            var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OTP token for email: {Email}", email);
            throw;
        }
    }

    public bool ValidateOtpToken(string otpToken, string otp, string email)
    {
        try
        {
            var tokenBytes = Convert.FromBase64String(otpToken);
            var tokenData = Encoding.UTF8.GetString(tokenBytes);
            var parts = tokenData.Split('|');

            if (parts.Length < 3) return false;

            var tokenEmail = parts[0];
            var tokenOtp = parts[1];
            var expiryString = parts[2];

            if (tokenEmail != email || tokenOtp != otp) return false;

            if (DateTime.TryParse(expiryString, out var expiry))
            {
                return DateTime.UtcNow <= expiry;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OTP token for email: {Email}", email);
            return false;
        }
    }
}