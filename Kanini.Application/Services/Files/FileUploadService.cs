using AutoMapper;
using Kanini.Application.DTOs.Files;
using Kanini.Application.Services.Files;
using Kanini.Common.Constants;
using Kanini.Common.Results;
using Kanini.Data.DatabaseContext;
using Kanini.Domain.Entities;
using Kanini.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Kanini.Application.Services.Files;

public class FileUploadService : IFileUploadService
{
    private readonly FhirConverterDbContext _context;
    private readonly IFileValidationService _validationService;
    private readonly ITempFileManager _tempFileManager;
    private readonly IMapper _mapper;
    private readonly ILogger<FileUploadService> _logger;
    private static readonly Dictionary<Guid, Guid?> _fileRequestMapping = new();

    public FileUploadService(
        FhirConverterDbContext context,
        IFileValidationService validationService,
        ITempFileManager tempFileManager,
        IMapper mapper,
        ILogger<FileUploadService> logger)
    {
        _context = context;
        _validationService = validationService;
        _tempFileManager = tempFileManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<FileUploadResponseDto>> UploadFileAsync(FileUploadInfo fileInfo, Guid? requestId, string uploadedBy, Guid userId)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.FileUploadStarted, fileInfo.FileName, fileInfo.Length);

            var validationResult = await ValidateFileAsync(fileInfo);
            if (validationResult.IsFailure)
            {
                return Result.Failure<FileUploadResponseDto>(validationResult.Error);
            }

            var detectedFormat = await _validationService.DetectFormatAsync(fileInfo.FileName, fileInfo.Content);
            if (detectedFormat.IsFailure)
            {
                return Result.Failure<FileUploadResponseDto>(detectedFormat.Error);
            }

            var fileId = Guid.NewGuid();
            var filePath = _tempFileManager.GetTempFilePath(fileId, fileInfo.FileName);

            // Store the requestId mapping
            _fileRequestMapping[fileId] = requestId;

            // Save file to temporary storage
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileInfo.Content.CopyToAsync(stream);
            }

            var response = new FileUploadResponseDto
            {
                FileId = fileId,
                OriginalFileName = fileInfo.FileName,
                DetectedFormat = detectedFormat.Value,
                FileSizeBytes = fileInfo.Length,
                Status = ConversionStatus.Processing, // File uploaded, ready for conversion
                UploadedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(MagicStrings.FileValidation.FileExpirationHours),
                RequestId = requestId
            };

            _logger.LogInformation(MagicStrings.LogMessages.FileUploadCompleted, fileId, filePath);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.FileUploadFailed, fileInfo.FileName, ex.Message);
            return Result.Failure<FileUploadResponseDto>(MagicStrings.ErrorMessages.FileUploadFailed);
        }
    }

    public async Task<Result> DeleteFileAsync(Guid fileId)
    {
        try
        {
            _tempFileManager.DeleteFile(fileId);
            _fileRequestMapping.Remove(fileId); // Clean up mapping
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return Result.Failure(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public static Guid? GetRequestIdForFile(Guid fileId)
    {
        return _fileRequestMapping.GetValueOrDefault(fileId);
    }

    public async Task<Result<FilePreviewResponseDto>> GetFilePreviewAsync(Guid fileId)
    {
        try
        {
            if (!_tempFileManager.FileExists(fileId))
            {
                return Result.Failure<FilePreviewResponseDto>(MagicStrings.ErrorMessages.FileExpired);
            }

            var fileInfo = _tempFileManager.GetFileInfo(fileId);
            var extension = Path.GetExtension(fileInfo.FileName).ToLowerInvariant();
            var format = extension switch
            {
                ".csv" => InputFormat.CSV,
                ".json" => InputFormat.JSON,
                ".xml" => InputFormat.CCDA,
                _ => InputFormat.CSV
            };

            var response = new FilePreviewResponseDto
            {
                FileId = fileId,
                OriginalFileName = fileInfo.FileName,
                Format = format,
                PreviewData = new List<Dictionary<string, object>>()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file preview {FileId}", fileId);
            return Result.Failure<FilePreviewResponseDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    private async Task<Result> ValidateFileAsync(FileUploadInfo fileInfo)
    {
        if (fileInfo == null || fileInfo.Length == 0)
        {
            return Result.Failure(MagicStrings.ErrorMessages.FileRequired);
        }

        if (fileInfo.Length > MagicStrings.FileValidation.MaxFileSizeBytes)
        {
            return Result.Failure(MagicStrings.ErrorMessages.FileTooLarge);
        }

        var extension = Path.GetExtension(fileInfo.FileName).ToLowerInvariant();
        if (!MagicStrings.FileValidation.AllowedExtensions.Contains(extension))
        {
            return Result.Failure(MagicStrings.ErrorMessages.UnsupportedFileType);
        }

        if (!MagicStrings.FileValidation.AllowedMimeTypes.Contains(fileInfo.ContentType))
        {
            return Result.Failure(MagicStrings.ErrorMessages.InvalidFileFormat);
        }

        return Result.Success();
    }
}