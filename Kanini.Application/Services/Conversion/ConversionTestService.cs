using Kanini.Application.Fhir;
using Kanini.Application.Parsers;
using Kanini.Application.DTOs.Conversion;
using Hl7.Fhir.Serialization;

namespace Kanini.Application.Services.Conversion;

public class ConversionTestService
{
    private readonly IJsonParser _jsonParser;
    private readonly IFhirConverter _fhirConverter;
    private readonly FhirJsonSerializer _serializer;

    public ConversionTestService(IJsonParser jsonParser, IFhirConverter fhirConverter)
    {
        _jsonParser = jsonParser;
        _fhirConverter = fhirConverter;
        _serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = true });
    }

    public async Task<string> TestConversionAsync(string jsonFilePath)
    {
        try
        {
            var jobId = Guid.NewGuid();
            
            // Step 1: Parse JSON to internal models
            var parseResult = await _jsonParser.ParseAsync(jsonFilePath, new List<FieldMappingDto>(), jobId);
            if (parseResult.IsFailure)
                return $"Parse failed: {parseResult.Error}";

            var (internalPatient, internalObservations) = parseResult.Value;

            // Step 2: Convert to FHIR resources
            var patientResult = _fhirConverter.ConvertPatient(internalPatient);
            if (patientResult.IsFailure)
                return $"Patient conversion failed: {patientResult.Error}";

            var patient = patientResult.Value;
            var observations = new List<Hl7.Fhir.Model.Observation>();

            foreach (var internalObservation in internalObservations)
            {
                var observationResult = _fhirConverter.ConvertObservation(internalObservation);
                if (observationResult.IsSuccess)
                    observations.Add(observationResult.Value);
            }

            // Step 3: Create FHIR Bundle
            var bundleResult = _fhirConverter.CreateBundle(patient, observations, jobId);
            if (bundleResult.IsFailure)
                return $"Bundle creation failed: {bundleResult.Error}";

            // Step 4: Serialize to JSON
            var fhirJson = _serializer.SerializeToString(bundleResult.Value);
            
            return fhirJson;
        }
        catch (Exception ex)
        {
            return $"Test failed: {ex.Message}";
        }
    }
}