using Kanini.Application.DTOs.Files;
using Kanini.Common.Results;

namespace Kanini.Application.Services.Files;

public interface IFileUploadService
{
    Task<Result<FileUploadResponseDto>> UploadFileAsync(FileUploadInfo fileInfo, Guid? requestId, string uploadedBy, Guid userId);
    Task<Result> DeleteFileAsync(Guid fileId);
    Task<Result<FilePreviewResponseDto>> GetFilePreviewAsync(Guid fileId);
}