using Kanini.Data.DatabaseContext;
using Kanini.Data.Repositories.Users;
using Kanini.Domain.Entities;
using Kanini.Common.Services;
using Microsoft.Extensions.Logging;

namespace Kanini.Data.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly FhirConverterDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(FhirConverterDbContext context, IEncryptionService encryptionService, ILogger<UserRepository> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            // Encrypt sensitive data before saving
            if (!string.IsNullOrEmpty(user.CreatedBy))
            {
                user.CreatedBy = _encryptionService.Encrypt(user.CreatedBy);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Decrypt for return
            if (!string.IsNullOrEmpty(user.CreatedBy))
            {
                user.CreatedBy = _encryptionService.Decrypt(user.CreatedBy);
            }
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", user.Email);
            throw;
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", user.UserId);
            throw;
        }
    }

    public async Task DeleteAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                user.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", userId);
            throw;
        }
    }
}