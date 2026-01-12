using Kanini.Common.Results;

namespace Kanini.Application.Services.Terminology;

public interface ITerminologyMappingService
{
    /// <summary>
    /// Maps test name to LOINC code
    /// </summary>
    /// <param name="testName">Input test name</param>
    /// <returns>System, Code, Display for LOINC</returns>
    (string system, string code, string display) ResolveObservationCode(string testName);

    /// <summary>
    /// Maps unit to UCUM code
    /// </summary>
    /// <param name="unit">Input unit</param>
    /// <returns>System, Code, Display for UCUM</returns>
    (string system, string code, string display) ResolveUnitCode(string unit);

    /// <summary>
    /// Validates if a test name has a known LOINC mapping
    /// </summary>
    bool HasLoincMapping(string testName);

    /// <summary>
    /// Validates if a unit has a known UCUM mapping
    /// </summary>
    bool HasUcumMapping(string unit);
}