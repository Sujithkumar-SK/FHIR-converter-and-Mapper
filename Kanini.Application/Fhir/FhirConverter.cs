using Hl7.Fhir.Model;
using Kanini.Application.Models;
using Kanini.Common.Results;

namespace Kanini.Application.Fhir;

public interface IFhirConverter
{
    Result<Patient> ConvertPatient(InternalPatient internalPatient);
    Result<Observation> ConvertObservation(InternalObservation internalObservation);
    Result<Bundle> CreateBundle(Patient patient, List<Observation> observations, Guid jobId);
}

public class FhirConverter : IFhirConverter
{
    public Result<Patient> ConvertPatient(InternalPatient internalPatient)
    {
        try
        {
            var patient = new Patient
            {
                Id = internalPatient.Id
            };

            // Add identifiers
            foreach (var identifier in internalPatient.Identifiers)
            {
                patient.Identifier.Add(new Identifier(identifier.System, identifier.Value));
            }

            // Add default identifier if none provided
            if (!patient.Identifier.Any())
            {
                patient.Identifier.Add(new Identifier("http://hospital.org/patient-id", internalPatient.Id));
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

            // Add birth date
            if (internalPatient.DateOfBirth.HasValue)
            {
                patient.BirthDate = internalPatient.DateOfBirth.Value.ToString("yyyy-MM-dd");
            }

            // Add gender
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
                Status = ParseObservationStatus(internalObservation.Status),
                Subject = new ResourceReference($"Patient/{internalObservation.PatientId}")
            };

            // Add code
            observation.Code = new CodeableConcept(
                internalObservation.System ?? "http://loinc.org",
                internalObservation.Code,
                internalObservation.Display ?? internalObservation.Code
            );

            // Add value
            if (internalObservation.ValueQuantity.HasValue)
            {
                observation.Value = new Quantity(
                    internalObservation.ValueQuantity.Value,
                    internalObservation.ValueUnit ?? ""
                );
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

            return Result.Success(observation);
        }
        catch (Exception ex)
        {
            return Result.Failure<Observation>($"Failed to convert observation: {ex.Message}");
        }
    }

    public Result<Bundle> CreateBundle(Patient patient, List<Observation> observations, Guid jobId)
    {
        try
        {
            var bundle = new Bundle
            {
                Id = $"bundle-{jobId}",
                Type = Bundle.BundleType.Collection,
                Timestamp = DateTimeOffset.UtcNow
            };

            // Add patient
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = $"Patient/{patient.Id}",
                Resource = patient
            });

            // Add observations
            foreach (var observation in observations)
            {
                bundle.Entry.Add(new Bundle.EntryComponent
                {
                    FullUrl = $"Observation/{observation.Id}",
                    Resource = observation
                });
            }

            return Result.Success(bundle);
        }
        catch (Exception ex)
        {
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