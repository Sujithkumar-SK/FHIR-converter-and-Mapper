using Kanini.Domain.Entities;

namespace Kanini.Data.Repositories.Users;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid userId);
}