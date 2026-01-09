using Kanini.Application.DTOs.Files;
using Kanini.Common.Results;
using Kanini.Domain.Enums;

namespace Kanini.Application.Services.Files;

public interface IFileValidationService
{
    Task<Result<FileValidationResultDto>> ValidateFileAsync(Guid fileId);
    Task<Result<InputFormat>> DetectFormatAsync(string fileName, Stream content);
    Task<Result<List<string>>> GetPreviewRecordsAsync(string filePath, InputFormat format);
}