namespace Kanini.Application.Services.Users;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}