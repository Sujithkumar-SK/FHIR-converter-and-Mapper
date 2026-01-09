using Kanini.Application.DTOs.Files;
using Kanini.Application.Services.Files;
using Kanini.Common.Constants;
using Kanini.Common.Results;
using Kanini.Data.DatabaseContext;
using Kanini.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Xml;

namespace Kanini.Application.Services.Files;

public class FileValidationService : IFileValidationService
{
    private readonly FhirConverterDbContext _context;
    private readonly ITempFileManager _tempFileManager;
    private readonly ILogger<FileValidationService> _logger;

    public FileValidationService(FhirConverterDbContext context, ITempFileManager tempFileManager, ILogger<FileValidationService> logger)
    {
        _context = context;
        _tempFileManager = tempFileManager;
        _logger = logger;
    }

    public async Task<Result<FileValidationResultDto>> ValidateFileAsync(Guid fileId)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.FileValidationStarted, fileId);

            var job = await _context.ConversionJobs.FindAsync(fileId);
            if (job == null)
            {
                return Result.Failure<FileValidationResultDto>(MagicStrings.ErrorMessages.FileNotFound);
            }

            if (!_tempFileManager.FileExists(fileId))
            {
                return Result.Failure<FileValidationResultDto>(MagicStrings.ErrorMessages.FileExpired);
            }

            // Get file path from temp file manager
            // For now, return basic validation without file content validation
            var validationResult = new FileValidationResultDto
            {
                FileId = fileId,
                DetectedFormat = job.InputFormat,
                ValidationErrors = new List<string>(),
                IsValid = true,
                RecordCount = 0,
                PreviewRecords = new List<string>()
            };

            _logger.LogInformation(MagicStrings.LogMessages.FileValidationCompleted, fileId, validationResult.IsValid ? "Valid" : "Invalid");
            return Result.Success(validationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.FileValidationFailed, fileId, ex.Message);
            return Result.Failure<FileValidationResultDto>(MagicStrings.ErrorMessages.FileValidationFailed);
        }
    }

    public async Task<Result<InputFormat>> DetectFormatAsync(string fileName, Stream content)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return extension switch
            {
                ".csv" => Result.Success(InputFormat.CSV),
                ".json" => Result.Success(InputFormat.JSON),
                ".xml" => Result.Success(InputFormat.CCDA),
                _ => Result.Failure<InputFormat>(MagicStrings.ErrorMessages.UnsupportedFileType)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting file format for {FileName}", fileName);
            return Result.Failure<InputFormat>(MagicStrings.ErrorMessages.InvalidFileFormat);
        }
    }

    public async Task<Result<List<string>>> GetPreviewRecordsAsync(string filePath, InputFormat format)
    {
        try
        {
            var previewRecords = new List<string>();
            const int maxPreviewRecords = 5;

            switch (format)
            {
                case InputFormat.CSV:
                    var csvLines = await File.ReadAllLinesAsync(filePath);
                    previewRecords.AddRange(csvLines.Take(maxPreviewRecords));
                    break;

                case InputFormat.JSON:
                    var jsonContent = await File.ReadAllTextAsync(filePath);
                    var jsonDoc = JsonDocument.Parse(jsonContent);
                    previewRecords.Add(jsonDoc.RootElement.ToString());
                    break;

                case InputFormat.CCDA:
                    var xmlContent = await File.ReadAllTextAsync(filePath);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlContent);
                    previewRecords.Add(xmlDoc.OuterXml.Substring(0, Math.Min(1000, xmlDoc.OuterXml.Length)));
                    break;
            }

            return Result.Success(previewRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preview records from {FilePath}", filePath);
            return Result.Failure<List<string>>(MagicStrings.ErrorMessages.FileValidationFailed);
        }
    }
}