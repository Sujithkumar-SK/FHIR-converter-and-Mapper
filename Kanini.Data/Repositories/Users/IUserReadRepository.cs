using Kanini.Domain.Entities;

namespace Kanini.Data.Repositories.Users;

public interface IUserReadRepository
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid userId);
    Task<IEnumerable<User>> GetAllAsync();
}