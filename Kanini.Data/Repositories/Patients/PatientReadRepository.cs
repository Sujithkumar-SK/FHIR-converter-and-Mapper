using Kanini.Common.Constants;
using Kanini.Data.Infrastructure;
using Kanini.Domain.Entities;
using Kanini.Common.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Kanini.Data.Repositories.Patients;

public class PatientReadRepository : IPatientReadRepository
{
    private readonly IDatabaseReader _databaseReader;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PatientReadRepository> _logger;

    public PatientReadRepository(IDatabaseReader databaseReader, IEncryptionService encryptionService, ILogger<PatientReadRepository> logger)
    {
        _databaseReader = databaseReader;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<IEnumerable<PatientIdentifier>> GetAllPatientsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all patients");

            var patients = await _databaseReader.QueryAsync<PatientIdentifier>(
                MagicStrings.StoredProcedures.GetAllPatients);

            _logger.LogInformation("Retrieved {Count} patients", patients.Count());
            return patients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all patients");
            throw;
        }
    }

    public async Task<PatientIdentifier?> GetByGlobalIdAsync(Guid globalPatientId)
    {
        try
        {
            var patients = await _databaseReader.QueryAsync<PatientIdentifier>(
                MagicStrings.StoredProcedures.GetPatientByGlobalId,
                new { GlobalPatientId = globalPatientId });

            return patients.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by global ID {GlobalPatientId}", globalPatientId);
            throw;
        }
    }

    public async Task<IEnumerable<PatientIdentifier>> GetByOrganizationAsync(Guid organizationId)
    {
        try
        {
            var patients = await _databaseReader.QueryAsync<PatientIdentifier>(
                MagicStrings.StoredProcedures.GetPatientsByOrganization,
                new { OrganizationId = organizationId });

            return patients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients for organization {OrganizationId}", organizationId);
            throw;
        }
    }

    private string ComputeHash(string input)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input.ToLowerInvariant()));
            return Convert.ToBase64String(hashedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.HashingFailed, input.Length);
            throw;
        }
    }
}