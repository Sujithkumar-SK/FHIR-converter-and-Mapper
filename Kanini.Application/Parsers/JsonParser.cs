using Kanini.Application.DTOs.Conversion;
using Kanini.Application.Models;
using Kanini.Common.Results;
using System.Text.Json;

namespace Kanini.Application.Parsers;

public interface IJsonParser
{
    Task<Result<(InternalPatient patient, List<InternalObservation> observations)>> ParseAsync(
        string filePath, 
        List<FieldMappingDto> fieldMappings,
        Guid jobId);
}

public class JsonParser : IJsonParser
{
    public async Task<Result<(InternalPatient patient, List<InternalObservation> observations)>> ParseAsync(
        string filePath, 
        List<FieldMappingDto> fieldMappings,
        Guid jobId)
    {
        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var jsonDoc = JsonDocument.Parse(jsonContent);

            // Parse single patient with observations
            var result = ParsePatientFromJson(jsonDoc.RootElement, jobId);
            if (result.IsFailure)
                return Result.Failure<(InternalPatient, List<InternalObservation>)>(result.Error);

            return Result.Success((result.Value.patient, result.Value.observations));
        }
        catch (Exception ex)
        {
            return Result.Failure<(InternalPatient, List<InternalObservation>)>($"JSON parsing failed: {ex.Message}");
        }
    }

    private Result<(InternalPatient patient, List<InternalObservation> observations)> ParsePatientFromJson(JsonElement patientElement, Guid jobId)
    {
        var patient = new InternalPatient();
        var observations = new List<InternalObservation>();

        // Parse patient ID
        if (patientElement.TryGetProperty("patientId", out var patientId))
            patient.Id = $"patient-{patientId.GetString()}";
        else
            patient.Id = $"patient-{jobId}";

        // Parse demographics
        if (patientElement.TryGetProperty("demographics", out var demographics))
        {
            if (demographics.TryGetProperty("firstName", out var firstName))
                patient.FirstName = firstName.GetString();
            if (demographics.TryGetProperty("lastName", out var lastName))
                patient.LastName = lastName.GetString();
            if (demographics.TryGetProperty("gender", out var gender))
                patient.Gender = gender.GetString();
            if (demographics.TryGetProperty("dateOfBirth", out var dob) && 
                DateTime.TryParse(dob.GetString(), out var parsedDob))
                patient.DateOfBirth = parsedDob;
        }

        // Parse lab results as observations
        if (patientElement.TryGetProperty("labResults", out var labResults))
        {
            foreach (var labResult in labResults.EnumerateArray())
            {
                var labObservations = ParseLabResults(labResult, patient.Id);
                observations.AddRange(labObservations);
            }
        }

        // Parse encounters for vitals
        if (patientElement.TryGetProperty("encounters", out var encounters))
        {
            foreach (var encounter in encounters.EnumerateArray())
            {
                if (encounter.TryGetProperty("vitals", out var vitals))
                {
                    var vitalObservations = ParseVitals(vitals, patient.Id);
                    observations.AddRange(vitalObservations);
                }
            }
        }

        return Result.Success((patient, observations));
    }

    private List<InternalObservation> ParseLabResults(JsonElement labResult, string patientId)
    {
        var observations = new List<InternalObservation>();

        if (labResult.TryGetProperty("results", out var results))
        {
            foreach (var result in results.EnumerateObject())
            {
                var observation = new InternalObservation
                {
                    PatientId = patientId,
                    Code = result.Name, // Test name for LOINC mapping
                    Display = result.Name,
                    EffectiveDateTime = DateTime.UtcNow
                };

                // Parse value and unit
                var valueStr = result.Value.GetString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var parts = valueStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && decimal.TryParse(parts[0], out var numericValue))
                    {
                        observation.ValueQuantity = numericValue;
                        if (parts.Length > 1)
                        {
                            observation.ValueUnit = string.Join(" ", parts.Skip(1));
                        }
                    }
                    else
                    {
                        observation.ValueString = valueStr;
                    }
                }

                observations.Add(observation);
            }
        }

        return observations;
    }

    private List<InternalObservation> ParseVitals(JsonElement vitals, string patientId)
    {
        var observations = new List<InternalObservation>();

        foreach (var vital in vitals.EnumerateObject())
        {
            var observation = new InternalObservation
            {
                PatientId = patientId,
                Code = vital.Name, // Test name for LOINC mapping
                Display = vital.Name,
                EffectiveDateTime = DateTime.UtcNow
            };

            // Parse value and unit
            var valueStr = vital.Value.GetString();
            if (!string.IsNullOrEmpty(valueStr))
            {
                var parts = valueStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && decimal.TryParse(parts[0], out var numericValue))
                {
                    observation.ValueQuantity = numericValue;
                    if (parts.Length > 1)
                    {
                        observation.ValueUnit = string.Join(" ", parts.Skip(1));
                    }
                }
                else
                {
                    observation.ValueString = valueStr;
                }
            }

            observations.Add(observation);
        }

        return observations;
    }
}