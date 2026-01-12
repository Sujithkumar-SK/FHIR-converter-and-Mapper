using Kanini.Application.DTOs.Conversion;
using Kanini.Application.Models;
using Kanini.Common.Results;

namespace Kanini.Application.Parsers;

public interface ICsvParser
{
    Task<Result<(InternalPatient patient, List<InternalObservation> observations)>> ParseAsync(
        string filePath, 
        List<FieldMappingDto> fieldMappings,
        Guid jobId);
}

public class CsvParser : ICsvParser
{
    public async Task<Result<(InternalPatient patient, List<InternalObservation> observations)>> ParseAsync(
        string filePath, 
        List<FieldMappingDto> fieldMappings,
        Guid jobId)
    {
        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length <= 1) 
                return Result.Success((new InternalPatient { Id = $"patient-{jobId}" }, new List<InternalObservation>()));

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var mappingDict = fieldMappings.ToDictionary(m => m.CsvColumn, m => m.FhirField);

            // Parse patient from first row
            var firstRowValues = lines[1].Split(',').Select(v => v.Trim()).ToArray();
            var firstRecord = new Dictionary<string, string>();
            for (int j = 0; j < Math.Min(headers.Length, firstRowValues.Length); j++)
            {
                firstRecord[headers[j]] = firstRowValues[j];
            }

            var patientResult = ParsePatient(firstRecord, mappingDict, jobId);
            if (patientResult.IsFailure)
                return Result.Failure<(InternalPatient, List<InternalObservation>)>(patientResult.Error);

            var patient = patientResult.Value;
            var observations = new List<InternalObservation>();

            // Parse observations from all rows
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',').Select(v => v.Trim()).ToArray();
                var record = new Dictionary<string, string>();

                for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                {
                    record[headers[j]] = values[j];
                }

                var observationResult = ParseObservation(record, mappingDict, patient.Id);
                if (observationResult.IsSuccess)
                {
                    observations.Add(observationResult.Value);
                }
            }

            return Result.Success((patient, observations));
        }
        catch (Exception ex)
        {
            return Result.Failure<(InternalPatient, List<InternalObservation>)>($"CSV parsing failed: {ex.Message}");
        }
    }

    private Result<InternalPatient> ParsePatient(Dictionary<string, string> record, Dictionary<string, string> mappings, Guid jobId)
    {
        var patient = new InternalPatient();

        if (TryGetValue(record, mappings, "patient.identifier", out var id))
            patient.Id = $"patient-{id}";
        else
            patient.Id = $"patient-{jobId}";

        TryGetValue(record, mappings, "patient.name.given", out var firstName);
        patient.FirstName = firstName;
        TryGetValue(record, mappings, "patient.name.family", out var lastName);
        patient.LastName = lastName;
        TryGetValue(record, mappings, "patient.gender", out var gender);
        patient.Gender = gender;
        TryGetValue(record, mappings, "patient.telecom.phone", out var phone);
        patient.Phone = phone;
        TryGetValue(record, mappings, "patient.telecom.email", out var email);
        patient.Email = email;

        if (TryGetValue(record, mappings, "patient.birthDate", out var birthDate) && 
            DateTime.TryParse(birthDate, out var parsedDate))
        {
            patient.DateOfBirth = parsedDate;
        }

        return Result.Success(patient);
    }

    private Result<InternalObservation> ParseObservation(Dictionary<string, string> record, Dictionary<string, string> mappings, string patientId)
    {
        // Try to get test name first (for LOINC mapping)
        string testName = string.Empty;
        if (TryGetValue(record, mappings, "observation.code", out var code))
        {
            testName = code;
        }
        else if (TryGetValue(record, mappings, "observation.display", out var display))
        {
            testName = display;
        }
        else
        {
            return Result.Failure<InternalObservation>("Observation code/test name is required");
        }

        var observation = new InternalObservation
        {
            Code = testName, // This will be mapped to LOINC by TerminologyService
            Display = testName,
            PatientId = patientId
        };

        // Handle value - try numeric first, then string
        if (TryGetValue(record, mappings, "observation.valueQuantity.value", out var value))
        {
            if (decimal.TryParse(value, out var numericValue))
            {
                observation.ValueQuantity = numericValue;
            }
            else
            {
                observation.ValueString = value;
            }
        }

        // Get unit if available (will be mapped to UCUM by TerminologyService)
        if (TryGetValue(record, mappings, "observation.valueQuantity.unit", out var valueUnit))
            observation.ValueUnit = valueUnit;

        if (TryGetValue(record, mappings, "observation.effectiveDateTime", out var effectiveDate) && 
            DateTime.TryParse(effectiveDate, out var parsedDate))
        {
            observation.EffectiveDateTime = parsedDate;
        }

        return Result.Success(observation);
    }



    private bool TryGetValue(Dictionary<string, string> record, Dictionary<string, string> mappings, string fhirField, out string value)
    {
        value = string.Empty;
        var csvColumn = mappings.FirstOrDefault(m => m.Value == fhirField).Key;
        if (csvColumn != null && record.ContainsKey(csvColumn))
        {
            value = record[csvColumn];
            return !string.IsNullOrWhiteSpace(value);
        }
        return false;
    }
}