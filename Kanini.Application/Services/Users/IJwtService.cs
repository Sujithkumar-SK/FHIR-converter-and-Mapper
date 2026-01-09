using Kanini.Domain.Entities;

namespace Kanini.Application.Services.Users;

public interface IJwtService
{
    string GenerateToken(User user);
}