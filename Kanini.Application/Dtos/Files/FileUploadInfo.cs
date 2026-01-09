namespace Kanini.Application.DTOs.Files;

public class FileUploadInfo
{
    public string FileName { get; set; } = null!;
    public long Length { get; set; }
    public string ContentType { get; set; } = null!;
    public Stream Content { get; set; } = null!;
}