using Kanini.Domain.Entities;

namespace Kanini.Data.Repositories.Patients;

public interface IPatientReadRepository
{
    Task<IEnumerable<PatientIdentifier>> GetAllPatientsAsync();
    Task<PatientIdentifier?> GetByGlobalIdAsync(Guid globalPatientId);
    Task<IEnumerable<PatientIdentifier>> GetByOrganizationAsync(Guid organizationId);
}