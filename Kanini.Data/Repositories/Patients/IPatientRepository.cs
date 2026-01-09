using Kanini.Domain.Entities;

namespace Kanini.Data.Repositories.Patients;

public interface IPatientRepository
{
    Task<PatientIdentifier> CreateAsync(PatientIdentifier patient);
    Task<PatientIdentifier> UpdateAsync(PatientIdentifier patient);
    Task DeleteAsync(Guid patientId);
}