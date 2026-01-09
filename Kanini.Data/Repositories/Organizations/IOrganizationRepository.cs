using Kanini.Domain.Entities;

namespace Kanini.Data.Repositories.Organizations;

public interface IOrganizationRepository
{
    Task<Organization> CreateAsync(Organization organization);
    Task<Organization> UpdateAsync(Organization organization);
    Task DeleteAsync(string organizationId);
}