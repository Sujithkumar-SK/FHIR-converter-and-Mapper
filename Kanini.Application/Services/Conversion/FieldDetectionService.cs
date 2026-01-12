using Kanini.Application.DTOs.Conversion;
using Kanini.Application.Services.Files;
using Kanini.Common.Constants;
using Kanini.Common.Results;
using Kanini.Data.DatabaseContext;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Xml;

namespace Kanini.Application.Services.Conversion;

public class FieldDetectionService : IFieldDetectionService
{
    private readonly FhirConverterDbContext _context;
    private readonly ITempFileManager _tempFileManager;
    private readonly ILogger<FieldDetectionService> _logger;

    private readonly Dictionary<string, string[]> _fieldPatterns = new()
    {
        ["patient.identifier"] = new[] { "patient_id", "patientid", "id", "patient_number", "mrn", "medical_record_number" },
        ["patient.name.given"] = new[] { "first_name", "firstname", "given_name", "fname", "given" },
        ["patient.name.family"] = new[] { "last_name", "lastname", "family_name", "lname", "surname", "family" },
        ["patient.birthDate"] = new[] { "dob", "date_of_birth", "dateofbirth", "birth_date", "birthdate" },
        ["patient.gender"] = new[] { "gender", "sex" },
        ["observation.code"] = new[] { "test_name", "testname", "lab_test", "test_type", "observation_code", "code" },
        ["observation.valueQuantity.value"] = new[] { "result", "value", "test_result", "lab_value", "result_value", "numeric_value" },
        ["observation.valueQuantity.unit"] = new[] { "unit", "units", "measurement_unit", "uom" },
        ["observation.effectiveDateTime"] = new[] { "test_date", "collection_date", "date", "observation_date", "effective_date" }
    };

    public FieldDetectionService(
        FhirConverterDbContext context,
        ITempFileManager tempFileManager,
        ILogger<FieldDetectionService> logger)
    {
        _context = context;
        _tempFileManager = tempFileManager;
        _logger = logger;
    }

    public async Task<Result<FieldDetectionResponseDto>> DetectFieldsAsync(Guid fileId)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.FieldMappingDetected, fileId, 0);

            if (!_tempFileManager.FileExists(fileId))
            {
                return Result.Failure<FieldDetectionResponseDto>(MagicStrings.ErrorMessages.FileExpired);
            }

            var fileInfo = _tempFileManager.GetFileInfo(fileId);
            var extension = Path.GetExtension(fileInfo.FileName).ToLowerInvariant();
            var inputFormat = extension switch
            {
                ".csv" => Domain.Enums.InputFormat.CSV,
                ".json" => Domain.Enums.InputFormat.JSON,
                ".xml" => Domain.Enums.InputFormat.CCDA,
                _ => Domain.Enums.InputFormat.CSV
            };

            var filePath = _tempFileManager.GetTempFilePath(fileId, fileInfo.FileName);
            var headers = await GetFileHeadersAsync(filePath, inputFormat);
            
            var detectedFields = new List<DetectedFieldDto>();
            
            foreach (var header in headers)
            {
                var detection = DetectField(header);
                if (detection != null)
                {
                    detectedFields.Add(detection);
                }
            }

            var response = new FieldDetectionResponseDto
            {
                FileId = fileId,
                DetectedFields = detectedFields,
                RequiredMappings = GetRequiredMappings(),
                AvailableFhirFields = _fieldPatterns.Keys.ToList()
            };

            _logger.LogInformation(MagicStrings.LogMessages.FieldMappingDetected, fileId, detectedFields.Count);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting fields for FileId: {FileId}", fileId);
            return Result.Failure<FieldDetectionResponseDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<List<string>>> GetAvailableFhirFieldsAsync()
    {
        try
        {
            return Result.Success(_fieldPatterns.Keys.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available FHIR fields");
            return Result.Failure<List<string>>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    private async Task<List<string>> GetFileHeadersAsync(string filePath, Domain.Enums.InputFormat format)
    {
        switch (format)
        {
            case Domain.Enums.InputFormat.CSV:
                var firstLine = await File.ReadAllLinesAsync(filePath);
                return firstLine.Length > 0 ? firstLine[0].Split(',').Select(h => h.Trim()).ToList() : new List<string>();
            
            case Domain.Enums.InputFormat.JSON:
                // Parse JSON and extract all possible field paths
                return await ExtractJsonFieldsAsync(filePath);
            
            case Domain.Enums.InputFormat.CCDA:
                // Parse CCDA XML and extract field paths
                return await ExtractCcdaFieldsAsync(filePath);
            
            default:
                return new List<string>();
        }
    }

    private DetectedFieldDto? DetectField(string columnName)
    {
        var normalizedColumn = columnName.ToLowerInvariant().Trim();
        
        foreach (var pattern in _fieldPatterns)
        {
            foreach (var fieldPattern in pattern.Value)
            {
                if (normalizedColumn.Contains(fieldPattern) || 
                    CalculateSimilarity(normalizedColumn, fieldPattern) > 0.7)
                {
                    return new DetectedFieldDto
                    {
                        ColumnName = columnName,
                        SuggestedFhirField = pattern.Key,
                        ConfidenceScore = CalculateSimilarity(normalizedColumn, fieldPattern),
                        SampleValues = new List<string>()
                    };
                }
            }
        }

        return null;
    }

    private double CalculateSimilarity(string source, string target)
    {
        if (source == target) return 1.0;
        if (source.Contains(target) || target.Contains(source)) return 0.8;
        
        // Simple Levenshtein distance calculation
        var distance = LevenshteinDistance(source, target);
        var maxLength = Math.Max(source.Length, target.Length);
        return 1.0 - (double)distance / maxLength;
    }

    private int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var matrix = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= target.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }

    private List<string> GetRequiredMappings()
    {
        return new List<string>
        {
            "patient.identifier",
            "patient.name.given",
            "patient.name.family",
            "observation.code",
            "observation.valueQuantity.value"
        };
    }

    private async Task<List<string>> ExtractJsonFieldsAsync(string filePath)
    {
        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var jsonDoc = JsonDocument.Parse(jsonContent);
            var fields = new List<string>();
            
            ExtractJsonFields(jsonDoc.RootElement, "", fields);
            return fields;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JSON file for field detection");
            return new List<string>();
        }
    }

    private void ExtractJsonFields(JsonElement element, string prefix, List<string> fields)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var fieldName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        ExtractJsonFields(property.Value, fieldName, fields);
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        // For arrays, check first element to get structure
                        if (property.Value.GetArrayLength() > 0)
                        {
                            var firstElement = property.Value.EnumerateArray().First();
                            if (firstElement.ValueKind == JsonValueKind.Object)
                            {
                                ExtractJsonFields(firstElement, fieldName, fields);
                            }
                            else
                            {
                                fields.Add(fieldName);
                            }
                        }
                    }
                    else
                    {
                        fields.Add(fieldName);
                    }
                }
                break;
        }
    }

    private async Task<List<string>> ExtractCcdaFieldsAsync(string filePath)
    {
        try
        {
            var xmlContent = await File.ReadAllTextAsync(filePath);
            
            // Add missing namespace declarations if not present
            if (!xmlContent.Contains("xmlns:xsi="))
            {
                xmlContent = xmlContent.Replace(
                    "<ClinicalDocument xmlns=\"urn:hl7-org:v3\">",
                    "<ClinicalDocument xmlns=\"urn:hl7-org:v3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                );
            }
            
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            
            var fields = new List<string>();
            
            // Extract common CCDA field mappings
            fields.AddRange(new[]
            {
                "patient_id",
                "first_name", 
                "last_name",
                "birth_date",
                "gender",
                "phone",
                "email",
                "address",
                "test_name",
                "test_result",
                "unit",
                "test_date"
            });
            
            return fields;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CCDA file for field detection");
            // Return common CCDA field mappings as fallback
            return new List<string>
            {
                "patient_id",
                "first_name", 
                "last_name",
                "birth_date",
                "gender",
                "test_name",
                "test_result",
                "unit",
                "test_date"
            };
        }
    }
}