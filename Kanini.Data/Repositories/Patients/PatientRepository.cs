using Kanini.Data.DatabaseContext;
using Kanini.Domain.Entities;
using Kanini.Common.Services;
using Kanini.Common.Constants;
using Microsoft.Extensions.Logging;

namespace Kanini.Data.Repositories.Patients;

public class PatientRepository : IPatientRepository
{
    private readonly FhirConverterDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PatientRepository> _logger;

    public PatientRepository(FhirConverterDbContext context, IEncryptionService encryptionService, ILogger<PatientRepository> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<PatientIdentifier> CreateAsync(PatientIdentifier patient)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.PatientCreationStarted, patient.GlobalPatientId);

            _context.PatientIdentifiers.Add(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation(MagicStrings.LogMessages.PatientCreationCompleted, patient.Id);
            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.PatientCreationFailed, patient.GlobalPatientId, ex.Message);
            throw;
        }
    }

    public async Task<PatientIdentifier> UpdateAsync(PatientIdentifier patient)
    {
        try
        {
            _context.PatientIdentifiers.Update(patient);
            await _context.SaveChangesAsync();
            
            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient with ID {PatientId}", patient.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid patientId)
    {
        try
        {
            var patient = await _context.PatientIdentifiers.FindAsync(patientId);
            if (patient != null)
            {
                _context.PatientIdentifiers.Remove(patient);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient with ID {PatientId}", patientId);
            throw;
        }
    }
}