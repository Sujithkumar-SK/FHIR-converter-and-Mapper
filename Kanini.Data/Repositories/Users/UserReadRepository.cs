using Kanini.Common.Constants;
using Kanini.Data.Infrastructure;
using Kanini.Data.Repositories.Users;
using Kanini.Domain.Entities;
using Kanini.Common.Services;

namespace Kanini.Data.Repositories.Users;

public class UserReadRepository : IUserReadRepository
{
    private readonly IDatabaseReader _databaseReader;
    private readonly IEncryptionService _encryptionService;

    public UserReadRepository(IDatabaseReader databaseReader, IEncryptionService encryptionService)
    {
        _databaseReader = databaseReader;
        _encryptionService = encryptionService;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        try
        {
            var result = await _databaseReader.QuerySingleOrDefaultAsync<bool>(
                MagicStrings.StoredProcedures.CheckUserExistsByEmail,
                new { Email = email });
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            var users = await _databaseReader.QueryAsync<User>(
                MagicStrings.StoredProcedures.GetUserByEmail,
                new { Email = email });
            
            var user = users.FirstOrDefault();
            if (user != null)
            {
                // Load organization if user has one
                if (user.OrganizationId.HasValue)
                {
                    // This would need to be implemented to load organization
                    // For now, we'll handle it in the service layer
                }
            }
            
            return user;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        try
        {
            var users = await _databaseReader.QueryAsync<User>(
                MagicStrings.StoredProcedures.GetUserById,
                new { UserId = userId });
            
            var user = users.FirstOrDefault();
            return user;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            var users = await _databaseReader.QueryAsync<User>(
                MagicStrings.StoredProcedures.GetAllUsers);
            
            return users;
        }
        catch (Exception)
        {
            throw;
        }
    }
}