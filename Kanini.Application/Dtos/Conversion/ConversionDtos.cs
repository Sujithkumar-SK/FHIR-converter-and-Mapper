namespace Kanini.Application.DTOs.Conversion;

public class FieldMappingDto
{
    public string CsvColumn { get; set; } = null!;
    public string FhirField { get; set; } = null!;
    public bool IsRequired { get; set; }
}

public class StartConversionRequestDto
{
    public Guid FileId { get; set; }
    public Guid? RequestId { get; set; }
    public List<FieldMappingDto> FieldMappings { get; set; } = new();
}

public class ConversionStatusResponseDto
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = null!;
    public int Progress { get; set; }
    public string? ErrorMessage { get; set; }
    public int PatientsProcessed { get; set; }
    public int ObservationsProcessed { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? ProcessingTimeMs { get; set; }
}

public class FhirBundlePreviewDto
{
    public Guid JobId { get; set; }
    public string BundleId { get; set; } = null!;
    public int PatientCount { get; set; }
    public int ObservationCount { get; set; }
    public List<string> PatientSample { get; set; } = new();
    public List<string> ObservationSample { get; set; } = new();
}