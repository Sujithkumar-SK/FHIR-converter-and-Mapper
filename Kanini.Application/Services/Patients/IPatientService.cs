using Kanini.Application.DTOs.Patients;
using Kanini.Common.Results;

namespace Kanini.Application.Services.Patients;

public interface IPatientService
{
    Task<Result<PatientResponseDto>> CreatePatientAsync(CreatePatientRequestDto request, string createdBy);
    Task<Result<IEnumerable<PatientResponseDto>>> GetAllPatientsAsync();
    Task<Result<PatientResponseDto>> GetPatientByGlobalIdAsync(Guid globalPatientId);
    Task<Result<IEnumerable<PatientResponseDto>>> GetPatientsByOrganizationAsync(Guid organizationId);
}