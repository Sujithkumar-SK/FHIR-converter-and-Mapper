using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.Files;

public class FileUploadResponseDto
{
    public Guid FileId { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public InputFormat DetectedFormat { get; set; }
    public ConversionStatus Status { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid? RequestId { get; set; }
}