namespace Kanini.Application.DTOs.Conversion;

public class DetectedFieldDto
{
    public string ColumnName { get; set; } = null!;
    public string SuggestedFhirField { get; set; } = null!;
    public double ConfidenceScore { get; set; }
    public List<string> SampleValues { get; set; } = new();
}

public class FieldDetectionResponseDto
{
    public Guid FileId { get; set; }
    public List<DetectedFieldDto> DetectedFields { get; set; } = new();
    public List<string> RequiredMappings { get; set; } = new();
    public List<string> AvailableFhirFields { get; set; } = new();
}