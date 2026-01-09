using Kanini.Application.Services.Users;
using Kanini.Common.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Kanini.Application.Services.Users;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var fromEmail = _configuration["EmailSettings:SenderEmail"];
            var appPassword = _configuration["EmailSettings:AppPassword"];
            var senderName = _configuration["EmailSettings:SenderName"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword))
            {
                // Extract OTP for development logging
                var otpMatch = System.Text.RegularExpressions.Regex.Match(body, @"<strong[^>]*>([0-9]{6})</strong>");
                var otp = otpMatch.Success ? otpMatch.Groups[1].Value : "Not found";
                
                _logger.LogWarning("=== EMAIL CONFIGURATION MISSING - DEVELOPMENT MODE ====");
                _logger.LogWarning("To: {To}", to);
                _logger.LogWarning("Subject: {Subject}", subject);
                _logger.LogWarning("OTP CODE: {OTP}", otp);
                _logger.LogWarning("======================================================");
                return;
            }

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, appPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, senderName ?? "FHIR Converter"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to: {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to: {To}", to);
            throw;
        }
    }
}