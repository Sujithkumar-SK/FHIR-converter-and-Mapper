using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.Files;

public class FileValidationResultDto
{
    public Guid FileId { get; set; }
    public bool IsValid { get; set; }
    public InputFormat DetectedFormat { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public int RecordCount { get; set; }
    public List<string> PreviewRecords { get; set; } = new();
}