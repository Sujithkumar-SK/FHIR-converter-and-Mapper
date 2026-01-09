using Kanini.Domain.Enums;

namespace Kanini.Application.DTOs.Files;

public class FilePreviewResponseDto
{
    public Guid FileId { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public InputFormat Format { get; set; }
    public int TotalRecords { get; set; }
    public List<string> Headers { get; set; } = new();
    public List<Dictionary<string, object>> PreviewData { get; set; } = new();
}