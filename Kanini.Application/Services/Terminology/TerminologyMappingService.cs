using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kanini.Application.Services.Terminology;

public class TerminologyMappingService : ITerminologyMappingService
{
    private readonly ILogger<TerminologyMappingService> _logger;
    private readonly HttpClient? _httpClient;
    private const string FhirTerminologyServer = "https://tx.fhir.org/r4";
    
    
    private readonly Dictionary<string, (string code, string display)> _loincMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Hematology
        ["Hemoglobin"] = ("718-7", "Hemoglobin [Mass/volume] in Blood"),
        ["HB"] = ("718-7", "Hemoglobin [Mass/volume] in Blood"),
        ["Hgb"] = ("718-7", "Hemoglobin [Mass/volume] in Blood"),
        ["Hb"] = ("718-7", "Hemoglobin [Mass/volume] in Blood"),
        ["Hematocrit"] = ("4544-3", "Hematocrit [Volume Fraction] of Blood by Automated count"),
        ["HCT"] = ("4544-3", "Hematocrit [Volume Fraction] of Blood by Automated count"),
        ["White Blood Cell Count"] = ("6690-2", "Leukocytes [#/volume] in Blood by Automated count"),
        ["WBC"] = ("6690-2", "Leukocytes [#/volume] in Blood by Automated count"),
        ["Red Blood Cell Count"] = ("789-8", "Erythrocytes [#/volume] in Blood by Automated count"),
        ["RBC"] = ("789-8", "Erythrocytes [#/volume] in Blood by Automated count"),
        ["Platelet Count"] = ("777-3", "Platelets [#/volume] in Blood by Automated count"),
        ["PLT"] = ("777-3", "Platelets [#/volume] in Blood by Automated count"),
        
        // Chemistry
        ["Glucose"] = ("2345-7", "Glucose [Mass/volume] in Serum or Plasma"),
        ["Blood Glucose"] = ("2345-7", "Glucose [Mass/volume] in Serum or Plasma"),
        ["Creatinine"] = ("2160-0", "Creatinine [Mass/volume] in Serum or Plasma"),
        ["Blood Urea Nitrogen"] = ("3094-0", "Urea nitrogen [Mass/volume] in Serum or Plasma"),
        ["BUN"] = ("3094-0", "Urea nitrogen [Mass/volume] in Serum or Plasma"),
        ["Sodium"] = ("2951-2", "Sodium [Moles/volume] in Serum or Plasma"),
        ["Potassium"] = ("2823-3", "Potassium [Moles/volume] in Serum or Plasma"),
        ["Chloride"] = ("2075-0", "Chloride [Moles/volume] in Serum or Plasma"),
        ["Total Cholesterol"] = ("2093-3", "Cholesterol [Mass/volume] in Serum or Plasma"),
        ["Cholesterol"] = ("2093-3", "Cholesterol [Mass/volume] in Serum or Plasma"),
        ["HDL Cholesterol"] = ("2085-9", "Cholesterol in HDL [Mass/volume] in Serum or Plasma"),
        ["LDL Cholesterol"] = ("2089-1", "Cholesterol in LDL [Mass/volume] in Serum or Plasma"),
        ["Triglycerides"] = ("2571-8", "Triglyceride [Mass/volume] in Serum or Plasma"),
        
        // Liver Function
        ["ALT"] = ("1742-6", "Alanine aminotransferase [Enzymatic activity/volume] in Serum or Plasma"),
        ["AST"] = ("1920-8", "Aspartate aminotransferase [Enzymatic activity/volume] in Serum or Plasma"),
        ["Bilirubin Total"] = ("1975-2", "Bilirubin.total [Mass/volume] in Serum or Plasma"),
        ["Total Bilirubin"] = ("1975-2", "Bilirubin.total [Mass/volume] in Serum or Plasma"),
        
        // Vitals
        ["Blood Pressure Systolic"] = ("8480-6", "Systolic blood pressure"),
        ["Systolic BP"] = ("8480-6", "Systolic blood pressure"),
        ["Blood Pressure Diastolic"] = ("8462-4", "Diastolic blood pressure"),
        ["Diastolic BP"] = ("8462-4", "Diastolic blood pressure"),
        ["Heart Rate"] = ("8867-4", "Heart rate"),
        ["Pulse"] = ("8867-4", "Heart rate"),
        ["Body Temperature"] = ("8310-5", "Body temperature"),
        ["Temperature"] = ("8310-5", "Body temperature"),
        ["Respiratory Rate"] = ("9279-1", "Respiratory rate"),
        ["Weight"] = ("29463-7", "Body weight"),
        ["Height"] = ("8302-2", "Body height"),
        ["BMI"] = ("39156-5", "Body mass index (BMI) [Ratio]"),
        ["Oxygen Saturation"] = ("2708-6", "Oxygen saturation in Arterial blood")
    };
    

    private readonly Dictionary<string, (string code, string display)> _ucumMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Mass/Volume
        ["g/dL"] = ("g/dL", "gram per deciliter"),
        ["mg/dL"] = ("mg/dL", "milligram per deciliter"),
        ["mg/L"] = ("mg/L", "milligram per liter"),
        ["g/L"] = ("g/L", "gram per liter"),
        ["ng/mL"] = ("ng/mL", "nanogram per milliliter"),
        ["pg/mL"] = ("pg/mL", "picogram per milliliter"),
        ["ug/dL"] = ("ug/dL", "microgram per deciliter"),
        ["ug/L"] = ("ug/L", "microgram per liter"),
        
        // Moles/Volume
        ["mmol/L"] = ("mmol/L", "millimole per liter"),
        ["umol/L"] = ("umol/L", "micromole per liter"),
        ["mEq/L"] = ("meq/L", "milliequivalent per liter"),
        
        // Count/Volume
        ["10*3/uL"] = ("10*3/uL", "thousand per microliter"),
        ["10*6/uL"] = ("10*6/uL", "million per microliter"),
        ["/uL"] = ("/uL", "per microliter"),
        ["cells/uL"] = ("/uL", "per microliter"),
        
        // Enzymatic Activity
        ["U/L"] = ("U/L", "unit per liter"),
        ["IU/L"] = ("[IU]/L", "international unit per liter"),
        
        // Pressure
        ["mm[Hg]"] = ("mm[Hg]", "millimeter of mercury"),
        ["mmHg"] = ("mm[Hg]", "millimeter of mercury"),
        
        // Rate
        ["bpm"] = ("/min", "per minute"),
        ["/min"] = ("/min", "per minute"),
        ["beats/min"] = ("/min", "per minute"),
        
        // Temperature
        ["Cel"] = ("Cel", "degree Celsius"),
        ["°C"] = ("Cel", "degree Celsius"),
        ["degC"] = ("Cel", "degree Celsius"),
        ["[degF]"] = ("[degF]", "degree Fahrenheit"),
        ["°F"] = ("[degF]", "degree Fahrenheit"),
        
        // Physical Measurements
        ["kg"] = ("kg", "kilogram"),
        ["lb"] = ("[lb_av]", "pound"),
        ["cm"] = ("cm", "centimeter"),
        ["m"] = ("m", "meter"),
        ["in"] = ("[in_i]", "inch"),
        ["ft"] = ("[ft_i]", "foot"),
        
        // Percentage
        ["%"] = ("%", "percent"),
        ["percent"] = ("%", "percent"),
        
        // Ratio
        ["kg/m2"] = ("kg/m2", "kilogram per square meter"),
        ["kg/m^2"] = ("kg/m2", "kilogram per square meter")
    };

    public TerminologyMappingService(ILogger<TerminologyMappingService> logger)
    {
        _logger = logger;
        try
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        }
        catch
        {
            _logger.LogWarning("Failed to initialize HttpClient, will use local cache only");
            _httpClient = null;
        }
    }

    public (string system, string code, string display) ResolveObservationCode(string testName)
    {
        if (string.IsNullOrWhiteSpace(testName))
        {
            _logger.LogWarning("Empty test name provided for LOINC mapping");
            return ("http://terminology.hl7.org/CodeSystem/data-absent-reason", "unknown", "Unknown");
        }

        var normalizedTestName = testName.Trim();
        
        if (_httpClient != null)
        {
            var apiResult = TryGetLoincFromApiAsync(normalizedTestName).GetAwaiter().GetResult();
            if (apiResult.HasValue)
            {
                _logger.LogInformation("LOINC code retrieved from API for '{TestName}': {Code}", testName, apiResult.Value.code);
                return ("http://loinc.org", apiResult.Value.code, apiResult.Value.display);
            }
        }
        
        if (_loincMappings.TryGetValue(normalizedTestName, out var mapping))
        {
            _logger.LogDebug("LOINC mapping found in local cache for '{TestName}': {Code}", testName, mapping.code);
            return ("http://loinc.org", mapping.code, mapping.display);
        }

        _logger.LogWarning("No LOINC mapping found for test name: '{TestName}', using text fallback", testName);
        return ("http://terminology.hl7.org/CodeSystem/observation-category", "survey", normalizedTestName);
    }

    public (string system, string code, string display) ResolveUnitCode(string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            return ("http://unitsofmeasure.org", "1", "1");
        }

        var normalizedUnit = unit.Trim();
        
        if (_ucumMappings.TryGetValue(normalizedUnit, out var mapping))
        {
            _logger.LogDebug("UCUM mapping found for '{Unit}': {Code}", unit, mapping.code);
            return ("http://unitsofmeasure.org", mapping.code, mapping.display);
        }

        // Fallback: use original unit as display with UCUM system
        _logger.LogWarning("No UCUM mapping found for unit: '{Unit}', using as-is", unit);
        return ("http://unitsofmeasure.org", normalizedUnit, normalizedUnit);
    }

    public bool HasLoincMapping(string testName)
    {
        return !string.IsNullOrWhiteSpace(testName) && 
               _loincMappings.ContainsKey(testName.Trim());
    }

    public bool HasUcumMapping(string unit)
    {
        return !string.IsNullOrWhiteSpace(unit) && 
               _ucumMappings.ContainsKey(unit.Trim());
    }

    private async Task<(string code, string display)?> TryGetLoincFromApiAsync(string testName)
    {
        if (_httpClient == null)
            return null;
            
        try
        {
            var searchUrl = $"{FhirTerminologyServer}/CodeSystem/$lookup?system=http://loinc.org&display={Uri.EscapeDataString(testName)}";
            
            var response = await _httpClient.GetAsync(searchUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                
                if (json.RootElement.TryGetProperty("parameter", out var parameters))
                {
                    string? code = null;
                    string? display = null;
                    
                    foreach (var param in parameters.EnumerateArray())
                    {
                        if (param.TryGetProperty("name", out var name))
                        {
                            if (name.GetString() == "code" && param.TryGetProperty("valueCode", out var valueCode))
                                code = valueCode.GetString();
                            if (name.GetString() == "display" && param.TryGetProperty("valueString", out var valueString))
                                display = valueString.GetString();
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(display))
                        return (code, display);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to retrieve LOINC from API for '{TestName}', using fallback", testName);
        }
        
        return null;
    }
}