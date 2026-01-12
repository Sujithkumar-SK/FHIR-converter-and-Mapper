using Hl7.Fhir.Model;
using Kanini.Application.Models;
using Kanini.Application.Services.Terminology;
using Kanini.Common.Results;
using Microsoft.Extensions.Logging;

namespace Kanini.Application.Fhir;

public interface IFhirConverter
{
    Result<Patient> ConvertPatient(InternalPatient internalPatient);
    Result<Observation> ConvertObservation(InternalObservation internalObservation);
    Result<Bundle> CreateBundle(Patient patient, List<Observation> observations, Guid jobId);
}

public class FhirConverter : IFhirConverter
{
    private readonly ITerminologyMappingService _terminologyService;
    private readonly ILogger<FhirConverter> _logger;

    public FhirConverter(ITerminologyMappingService terminologyService, ILogger<FhirConverter> logger)
    {
        _terminologyService = terminologyService;
        _logger = logger;
    }
    public Result<Patient> ConvertPatient(InternalPatient internalPatient)
    {
        try
        {
            // FHIR Rule: Patient.id = patient-{patientIdentifier}
            var patientId = internalPatient.Id.StartsWith("patient-") ? internalPatient.Id : $"patient-{internalPatient.Id}";
            
            var patient = new Patient
            {
                Id = patientId
            };

            // Add identifiers with meaningful system
            foreach (var identifier in internalPatient.Identifiers)
            {
                patient.Identifier.Add(new Identifier(identifier.System, identifier.Value));
            }

            // Add default identifier if none provided - use meaningful system
            if (!patient.Identifier.Any())
            {
                patient.Identifier.Add(new Identifier("http://hospital.org/patient-id", internalPatient.Id.Replace("patient-", "")));
            }

            // Add name
            if (!string.IsNullOrEmpty(internalPatient.FirstName) || !string.IsNullOrEmpty(internalPatient.LastName))
            {
                var name = new HumanName();
                if (!string.IsNullOrEmpty(internalPatient.FirstName))
                    name.Given = new[] { internalPatient.FirstName };
                if (!string.IsNullOrEmpty(internalPatient.LastName))
                    name.Family = internalPatient.LastName;
                patient.Name.Add(name);
            }

            // Add birth date - must be yyyy-MM-dd format
            if (internalPatient.DateOfBirth.HasValue)
            {
                patient.BirthDate = internalPatient.DateOfBirth.Value.ToString("yyyy-MM-dd");
            }

            // Add gender - must be valid FHIR enum
            patient.Gender = ParseGender(internalPatient.Gender);

            // Add contact info
            if (!string.IsNullOrEmpty(internalPatient.Phone) || !string.IsNullOrEmpty(internalPatient.Email))
            {
                var telecom = new List<ContactPoint>();
                if (!string.IsNullOrEmpty(internalPatient.Phone))
                {
                    telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Home, internalPatient.Phone));
                }
                if (!string.IsNullOrEmpty(internalPatient.Email))
                {
                    telecom.Add(new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Home, internalPatient.Email));
                }
                patient.Telecom = telecom;
            }

            // Add address
            if (internalPatient.Address != null)
            {
                var address = new Address
                {
                    Line = !string.IsNullOrEmpty(internalPatient.Address.Street) ? new[] { internalPatient.Address.Street } : null,
                    City = internalPatient.Address.City,
                    State = internalPatient.Address.State,
                    PostalCode = internalPatient.Address.ZipCode
                };
                patient.Address.Add(address);
            }

            return Result.Success(patient);
        }
        catch (Exception ex)
        {
            return Result.Failure<Patient>($"Failed to convert patient: {ex.Message}");
        }
    }

    public Result<Observation> ConvertObservation(InternalObservation internalObservation)
    {
        try
        {
            var observation = new Observation
            {
                Id = internalObservation.Id,
                Status = ParseObservationStatus(internalObservation.Status)
            };

            // FHIR Rule: Observation.subject MUST reference Patient
            var patientId = internalObservation.PatientId.StartsWith("patient-") ? internalObservation.PatientId : $"patient-{internalObservation.PatientId}";
            observation.Subject = new ResourceReference($"Patient/{patientId}");

            // FHIR Rule: Observation.code MUST use LOINC
            var (codeSystem, loincCode, codeDisplay) = _terminologyService.ResolveObservationCode(internalObservation.Code);
            observation.Code = new CodeableConcept
            {
                Coding = new List<Coding>
                {
                    new Coding
                    {
                        System = codeSystem,
                        Code = loincCode,
                        Display = codeDisplay
                    }
                }
            };

            // Add value with proper UCUM units
            var (unitSystem, ucumCode, unitDisplay) = _terminologyService.ResolveUnitCode(internalObservation.ValueUnit ?? "");
            
            if (internalObservation.ValueQuantity.HasValue)
            {
                // FHIR Rule: Observation.valueQuantity MUST use UCUM
                observation.Value = new Quantity
                {
                    Value = internalObservation.ValueQuantity.Value,
                    Unit = unitDisplay,
                    System = unitSystem,
                    Code = ucumCode
                };
            }
            else if (!string.IsNullOrEmpty(internalObservation.ValueString))
            {
                observation.Value = new FhirString(internalObservation.ValueString);
            }

            // Add effective date
            if (internalObservation.EffectiveDateTime.HasValue)
            {
                observation.Effective = new FhirDateTime(internalObservation.EffectiveDateTime.Value);
            }

            _logger.LogDebug("Converted observation with LOINC code {LoincCode} and UCUM unit {UcumCode}", loincCode, ucumCode);
            return Result.Success(observation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert observation: {Error}", ex.Message);
            return Result.Failure<Observation>($"Failed to convert observation: {ex.Message}");
        }
    }

    public Result<Bundle> CreateBundle(Patient patient, List<Observation> observations, Guid jobId)
    {
        try
        {
            // FHIR Rule: Bundle type = collection
            var bundle = new Bundle
            {
                Id = $"bundle-{jobId}",
                Type = Bundle.BundleType.Collection,
                Timestamp = DateTimeOffset.UtcNow
            };

            // FHIR Rule: Bundle contains 1 Patient + N Observations
            // Add patient first
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = $"Patient/{patient.Id}",
                Resource = patient
            });

            // Add observations - all must reference the same patient
            foreach (var observation in observations)
            {
                // Validate that observation references the correct patient
                if (observation.Subject?.Reference != $"Patient/{patient.Id}")
                {
                    _logger.LogWarning("Observation {ObsId} does not reference correct patient {PatientId}", 
                        observation.Id, patient.Id);
                }
                
                bundle.Entry.Add(new Bundle.EntryComponent
                {
                    FullUrl = $"Observation/{observation.Id}",
                    Resource = observation
                });
            }

            _logger.LogInformation("Created FHIR Bundle with 1 Patient and {ObservationCount} Observations", observations.Count);
            return Result.Success(bundle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create FHIR bundle: {Error}", ex.Message);
            return Result.Failure<Bundle>($"Failed to create bundle: {ex.Message}");
        }
    }

    private AdministrativeGender ParseGender(string? gender)
    {
        return gender?.ToLowerInvariant() switch
        {
            "male" or "m" => AdministrativeGender.Male,
            "female" or "f" => AdministrativeGender.Female,
            "other" or "o" => AdministrativeGender.Other,
            _ => AdministrativeGender.Unknown
        };
    }

    private ObservationStatus ParseObservationStatus(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "final" => ObservationStatus.Final,
            "preliminary" => ObservationStatus.Preliminary,
            "cancelled" => ObservationStatus.Cancelled,
            _ => ObservationStatus.Final
        };
    }
}