using Kanini.Common.Constants;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Kanini.Application.Services.Files;

public interface ITempFileManager
{
    string GetTempFilePath(Guid fileId, string originalFileName);
    bool FileExists(Guid fileId);
    void DeleteFile(Guid fileId);
    void CleanupExpiredFiles();
    (string FileName, long Size) GetFileInfo(Guid fileId);
}

public class TempFileManager : ITempFileManager
{
    private readonly ConcurrentDictionary<Guid, (string FilePath, string OriginalFileName, DateTime CreatedAt)> _fileRegistry;
    private readonly string _tempDirectory;
    private readonly ILogger<TempFileManager> _logger;

    public TempFileManager(ILogger<TempFileManager> logger)
    {
        _fileRegistry = new ConcurrentDictionary<Guid, (string, string, DateTime)>();
        _tempDirectory = Path.Combine(Path.GetTempPath(), MagicStrings.FileValidation.TempFolderName);
        _logger = logger;
        Directory.CreateDirectory(_tempDirectory);
    }

    public string GetTempFilePath(Guid fileId, string originalFileName)
    {
        var fileName = $"{fileId}_{Path.GetFileName(originalFileName)}";
        var filePath = Path.Combine(_tempDirectory, fileName);
        
        _fileRegistry.TryAdd(fileId, (filePath, originalFileName, DateTime.UtcNow));
        return filePath;
    }

    public bool FileExists(Guid fileId)
    {
        if (_fileRegistry.TryGetValue(fileId, out var fileInfo))
        {
            return File.Exists(fileInfo.FilePath);
        }
        return false;
    }

    public void DeleteFile(Guid fileId)
    {
        if (_fileRegistry.TryRemove(fileId, out var fileInfo))
        {
            try
            {
                if (File.Exists(fileInfo.FilePath))
                {
                    File.Delete(fileInfo.FilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temp file {FilePath}", fileInfo.FilePath);
            }
        }
    }

    public (string FileName, long Size) GetFileInfo(Guid fileId)
    {
        if (_fileRegistry.TryGetValue(fileId, out var fileInfo) && File.Exists(fileInfo.FilePath))
        {
            var size = new FileInfo(fileInfo.FilePath).Length;
            return (fileInfo.OriginalFileName, size);
        }
        return (string.Empty, 0);
    }

    public void CleanupExpiredFiles()
    {
        var expiredFiles = _fileRegistry
            .Where(kvp => kvp.Value.CreatedAt < DateTime.UtcNow.AddHours(-MagicStrings.FileValidation.FileExpirationHours))
            .ToList();

        foreach (var expiredFile in expiredFiles)
        {
            DeleteFile(expiredFile.Key);
        }
    }
}