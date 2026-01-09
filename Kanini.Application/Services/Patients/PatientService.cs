using AutoMapper;
using Kanini.Application.DTOs.Patients;
using Kanini.Application.Services.Patients;
using Kanini.Common.Constants;
using Kanini.Common.Results;
using Kanini.Common.Services;
using Kanini.Data.Repositories.Patients;
using Kanini.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Kanini.Application.Services.Patients;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IPatientReadRepository _patientReadRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMapper _mapper;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        IPatientRepository patientRepository,
        IPatientReadRepository patientReadRepository,
        IEncryptionService encryptionService,
        IMapper mapper,
        ILogger<PatientService> logger)
    {
        _patientRepository = patientRepository;
        _patientReadRepository = patientReadRepository;
        _encryptionService = encryptionService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PatientResponseDto>> CreatePatientAsync(CreatePatientRequestDto request, string createdBy)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.PatientCreationStarted, Guid.NewGuid());

            // Check if patient already exists in this organization
            var existingPatients = await _patientReadRepository.GetByOrganizationAsync(request.SourceOrganizationId);
            
            var existingInOrg = existingPatients.FirstOrDefault(p => 
                _encryptionService.Decrypt(p.LocalPatientId) == request.LocalPatientId);
            
            if (existingInOrg != null)
            {
                return Result.Failure<PatientResponseDto>(MagicStrings.ErrorMessages.PatientAlreadyExists);
            }

            var globalPatientId = Guid.NewGuid();

            // Create patient entity
            var patient = _mapper.Map<PatientIdentifier>(request);
            patient.GlobalPatientId = globalPatientId;
            patient.CreatedBy = createdBy;

            // Encrypt sensitive fields
            patient.LocalPatientId = _encryptionService.Encrypt(patient.LocalPatientId);
            if (!string.IsNullOrEmpty(patient.FirstName))
                patient.FirstName = _encryptionService.Encrypt(patient.FirstName);
            if (!string.IsNullOrEmpty(patient.LastName))
                patient.LastName = _encryptionService.Encrypt(patient.LastName);

            // Generate hashes for searching
            if (!string.IsNullOrEmpty(request.FirstName))
                patient.FirstNameHash = ComputeHash(request.FirstName);
            patient.LastNameHash = ComputeHash(request.LastName);
            if (request.DateOfBirth.HasValue)
                patient.DateOfBirthHash = ComputeHash(request.DateOfBirth.Value.ToString("yyyy-MM-dd"));

            // Save patient
            var createdPatient = await _patientRepository.CreateAsync(patient);

            // Decrypt for response
            createdPatient.LocalPatientId = _encryptionService.Decrypt(createdPatient.LocalPatientId);
            if (!string.IsNullOrEmpty(createdPatient.FirstName))
                createdPatient.FirstName = _encryptionService.Decrypt(createdPatient.FirstName);
            if (!string.IsNullOrEmpty(createdPatient.LastName))
                createdPatient.LastName = _encryptionService.Decrypt(createdPatient.LastName);

            var response = _mapper.Map<PatientResponseDto>(createdPatient);
            response.Message = MagicStrings.SuccessMessages.PatientCreated;

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.PatientCreationFailed, Guid.Empty, ex.Message);
            return Result.Failure<PatientResponseDto>(MagicStrings.ErrorMessages.PatientCreationFailed);
        }
    }

    public async Task<Result<IEnumerable<PatientResponseDto>>> GetAllPatientsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all patients");

            var patients = await _patientReadRepository.GetAllPatientsAsync();

            var decryptedPatients = new List<PatientIdentifier>();
            foreach (var patient in patients)
            {
                // Decrypt sensitive fields
                patient.LocalPatientId = _encryptionService.Decrypt(patient.LocalPatientId);
                if (!string.IsNullOrEmpty(patient.FirstName))
                    patient.FirstName = _encryptionService.Decrypt(patient.FirstName);
                if (!string.IsNullOrEmpty(patient.LastName))
                    patient.LastName = _encryptionService.Decrypt(patient.LastName);
                
                decryptedPatients.Add(patient);
            }

            var response = _mapper.Map<IEnumerable<PatientResponseDto>>(decryptedPatients);
            
            _logger.LogInformation("Retrieved {Count} patients", patients.Count());
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all patients");
            return Result.Failure<IEnumerable<PatientResponseDto>>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<PatientResponseDto>> GetPatientByGlobalIdAsync(Guid globalPatientId)
    {
        try
        {
            var patient = await _patientReadRepository.GetByGlobalIdAsync(globalPatientId);
            if (patient == null)
            {
                return Result.Failure<PatientResponseDto>(MagicStrings.ErrorMessages.PatientNotFound);
            }

            // Decrypt sensitive fields
            patient.LocalPatientId = _encryptionService.Decrypt(patient.LocalPatientId);
            if (!string.IsNullOrEmpty(patient.FirstName))
                patient.FirstName = _encryptionService.Decrypt(patient.FirstName);
            if (!string.IsNullOrEmpty(patient.LastName))
                patient.LastName = _encryptionService.Decrypt(patient.LastName);

            var response = _mapper.Map<PatientResponseDto>(patient);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by global ID {GlobalPatientId}", globalPatientId);
            return Result.Failure<PatientResponseDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<PatientResponseDto>>> GetPatientsByOrganizationAsync(Guid organizationId)
    {
        try
        {
            var patients = await _patientReadRepository.GetByOrganizationAsync(organizationId);
            
            var decryptedPatients = new List<PatientIdentifier>();
            foreach (var patient in patients)
            {
                // Decrypt sensitive fields
                patient.LocalPatientId = _encryptionService.Decrypt(patient.LocalPatientId);
                if (!string.IsNullOrEmpty(patient.FirstName))
                    patient.FirstName = _encryptionService.Decrypt(patient.FirstName);
                if (!string.IsNullOrEmpty(patient.LastName))
                    patient.LastName = _encryptionService.Decrypt(patient.LastName);
                
                decryptedPatients.Add(patient);
            }

            var response = _mapper.Map<IEnumerable<PatientResponseDto>>(decryptedPatients);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients for organization {OrganizationId}", organizationId);
            return Result.Failure<IEnumerable<PatientResponseDto>>(MagicStrings.ErrorMessages.InternalServerError);
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