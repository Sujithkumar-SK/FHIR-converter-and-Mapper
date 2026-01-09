using Kanini.Data.DatabaseContext;
using Kanini.Data.Repositories.Organizations;
using Kanini.Domain.Entities;
using Kanini.Common.Services;
using Microsoft.Extensions.Logging;

namespace Kanini.Data.Repositories.Organizations;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly FhirConverterDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<OrganizationRepository> _logger;

    public OrganizationRepository(FhirConverterDbContext context, IEncryptionService encryptionService, ILogger<OrganizationRepository> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<Organization> CreateAsync(Organization organization)
    {
        try
        {
            // Encrypt sensitive data before saving
            if (!string.IsNullOrEmpty(organization.Name))
            {
                organization.Name = _encryptionService.Encrypt(organization.Name);
            }
            if (!string.IsNullOrEmpty(organization.ContactEmail))
            {
                organization.ContactEmail = _encryptionService.Encrypt(organization.ContactEmail);
            }
            if (!string.IsNullOrEmpty(organization.ContactPhone))
            {
                organization.ContactPhone = _encryptionService.Encrypt(organization.ContactPhone);
            }
            if (!string.IsNullOrEmpty(organization.CreatedBy))
            {
                organization.CreatedBy = _encryptionService.Encrypt(organization.CreatedBy);
            }

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();
            
            // Decrypt for return
            if (!string.IsNullOrEmpty(organization.Name))
            {
                organization.Name = _encryptionService.Decrypt(organization.Name);
            }
            if (!string.IsNullOrEmpty(organization.ContactEmail))
            {
                organization.ContactEmail = _encryptionService.Decrypt(organization.ContactEmail);
            }
            if (!string.IsNullOrEmpty(organization.ContactPhone))
            {
                organization.ContactPhone = _encryptionService.Decrypt(organization.ContactPhone);
            }
            if (!string.IsNullOrEmpty(organization.CreatedBy))
            {
                organization.CreatedBy = _encryptionService.Decrypt(organization.CreatedBy);
            }
            
            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization with ID {OrganizationId}", organization.OrganizationId);
            throw;
        }
    }

    public async Task<Organization> UpdateAsync(Organization organization)
    {
        try
        {
            // Encrypt sensitive data before saving
            if (!string.IsNullOrEmpty(organization.Name))
            {
                organization.Name = _encryptionService.Encrypt(organization.Name);
            }
            if (!string.IsNullOrEmpty(organization.ContactEmail))
            {
                organization.ContactEmail = _encryptionService.Encrypt(organization.ContactEmail);
            }
            if (!string.IsNullOrEmpty(organization.ContactPhone))
            {
                organization.ContactPhone = _encryptionService.Encrypt(organization.ContactPhone);
            }

            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();
            
            // Decrypt for return
            if (!string.IsNullOrEmpty(organization.Name))
            {
                organization.Name = _encryptionService.Decrypt(organization.Name);
            }
            if (!string.IsNullOrEmpty(organization.ContactEmail))
            {
                organization.ContactEmail = _encryptionService.Decrypt(organization.ContactEmail);
            }
            if (!string.IsNullOrEmpty(organization.ContactPhone))
            {
                organization.ContactPhone = _encryptionService.Decrypt(organization.ContactPhone);
            }
            
            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization with ID {OrganizationId}", organization.OrganizationId);
            throw;
        }
    }

    public async Task DeleteAsync(string organizationId)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization != null)
            {
                organization.IsActive = false;
                organization.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization with ID {OrganizationId}", organizationId);
            throw;
        }
    }
}