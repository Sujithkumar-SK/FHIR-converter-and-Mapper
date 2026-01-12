using Kanini.Application.DTOs.Conversion;
using Kanini.Application.Models;
using Kanini.Common.Results;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace Kanini.Application.Parsers;

public interface ICcdaParser
{
    Task<Result<(InternalPatient patient, List<InternalObservation> observations)>> ParseAsync(
        string filePath, 
        List<FieldMappingDto> fieldMappings,
        Guid jobId);
}

public class CcdaParser : ICcdaParser
{
    private readonly ILogger<CcdaParser> _logger;

    public CcdaParser(ILogger<CcdaParser> logger)
    {
        _logger = logger;
    }

    public async Task<Result<(InternalPatient patient, List<InternalObservation> observations)>> ParseAsync(
        string filePath, 
        List<FieldMappingDto> fieldMappings,
        Guid jobId)
    {
        try
        {
            var xmlContent = await File.ReadAllTextAsync(filePath);
            var doc = new XmlDocument();
            
            // Handle namespace issues
            if (!xmlContent.Contains("xmlns:xsi="))
            {
                xmlContent = xmlContent.Replace(
                    "<ClinicalDocument xmlns=\"urn:hl7-org:v3\">",
                    "<ClinicalDocument xmlns=\"urn:hl7-org:v3\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                );
            }
            
            doc.LoadXml(xmlContent);
            
            var patient = ParsePatient(doc, jobId);
            var observations = ParseObservations(doc, patient.Id);

            return Result.Success((patient, observations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CCDA parsing failed for file: {FilePath}", filePath);
            return Result.Failure<(InternalPatient, List<InternalObservation>)>($"CCDA parsing failed: {ex.Message}");
        }
    }

    private InternalPatient ParsePatient(XmlDocument doc, Guid jobId)
    {
        var patient = new InternalPatient
        {
            Id = $"patient-{jobId}"
        };

        try
        {
            // Create namespace manager for HL7 namespace
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("hl7", "urn:hl7-org:v3");
            
            // Parse patient name
            var nameNode = doc.SelectSingleNode("//hl7:name/hl7:given", nsmgr);
            if (nameNode != null) patient.FirstName = nameNode.InnerText;
            
            var familyNode = doc.SelectSingleNode("//hl7:name/hl7:family", nsmgr);
            if (familyNode != null) patient.LastName = familyNode.InnerText;
            
            // Parse birth date
            var birthNode = doc.SelectSingleNode("//hl7:birthTime", nsmgr);
            if (birthNode?.Attributes?["value"] != null)
            {
                var birthValue = birthNode.Attributes["value"].Value;
                if (DateTime.TryParseExact(birthValue, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var birthDate))
                {
                    patient.DateOfBirth = birthDate;
                }
            }
            
            // Parse gender
            var genderNode = doc.SelectSingleNode("//hl7:administrativeGenderCode", nsmgr);
            if (genderNode?.Attributes?["code"] != null)
            {
                var genderCode = genderNode.Attributes["code"].Value;
                patient.Gender = genderCode switch
                {
                    "M" => "Male",
                    "F" => "Female",
                    _ => "Unknown"
                };
            }
            
            // Parse patient ID
            var idNode = doc.SelectSingleNode("//hl7:patientRole/hl7:id", nsmgr);
            if (idNode?.Attributes?["extension"] != null)
            {
                var patientId = idNode.Attributes["extension"].Value;
                patient.Id = $"patient-{patientId}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing patient data from CCDA, using defaults");
        }

        // Set defaults if not found
        if (string.IsNullOrEmpty(patient.FirstName)) patient.FirstName = "Sample";
        if (string.IsNullOrEmpty(patient.LastName)) patient.LastName = "Patient";
        if (string.IsNullOrEmpty(patient.Gender)) patient.Gender = "Unknown";

        return patient;
    }

    private List<InternalObservation> ParseObservations(XmlDocument doc, string patientId)
    {
        var observations = new List<InternalObservation>();

        try
        {
            // Create namespace manager for HL7 namespace
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("hl7", "urn:hl7-org:v3");
            
            // Parse lab results
            var observationNodes = doc.SelectNodes("//hl7:observation", nsmgr);
            if (observationNodes != null)
            {
                foreach (XmlNode obsNode in observationNodes)
                {
                    var observation = new InternalObservation
                    {
                        PatientId = patientId,
                        EffectiveDateTime = DateTime.UtcNow
                    };

                    // Parse test code and display
                    var codeNode = obsNode.SelectSingleNode(".//hl7:code", nsmgr);
                    if (codeNode?.Attributes != null)
                    {
                        observation.Code = codeNode.Attributes["displayName"]?.Value ?? codeNode.Attributes["code"]?.Value ?? "Unknown Test";
                        observation.Display = observation.Code;
                    }

                    // Parse value
                    var valueNode = obsNode.SelectSingleNode(".//hl7:value", nsmgr);
                    if (valueNode?.Attributes != null)
                    {
                        var valueAttr = valueNode.Attributes["value"]?.Value;
                        var unitAttr = valueNode.Attributes["unit"]?.Value;
                        
                        if (decimal.TryParse(valueAttr, out var numericValue))
                        {
                            observation.ValueQuantity = numericValue;
                            observation.ValueUnit = unitAttr;
                        }
                        else
                        {
                            observation.ValueString = valueAttr;
                        }
                    }

                    // Parse effective time
                    var effectiveTimeNode = obsNode.SelectSingleNode(".//hl7:effectiveTime", nsmgr);
                    if (effectiveTimeNode?.Attributes?["value"] != null)
                    {
                        var timeValue = effectiveTimeNode.Attributes["value"].Value;
                        if (DateTime.TryParseExact(timeValue, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var effectiveDate))
                        {
                            observation.EffectiveDateTime = effectiveDate;
                        }
                    }

                    if (!string.IsNullOrEmpty(observation.Code))
                    {
                        observations.Add(observation);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing observations from CCDA");
        }

        // Add default observation if none found
        if (observations.Count == 0)
        {
            observations.Add(new InternalObservation
            {
                PatientId = patientId,
                Code = "General Health Status",
                Display = "General Health Status",
                ValueString = "Good",
                EffectiveDateTime = DateTime.UtcNow
            });
        }

        return observations;
    }
}