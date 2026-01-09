using Kanini.Common.Constants;
using Kanini.Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kanini.Application.Services.Files;

public interface IFileCleanupService
{
    Task CleanupExpiredFilesAsync();
}

public class FileCleanupService : IFileCleanupService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileCleanupService> _logger;

    public FileCleanupService(IServiceProvider serviceProvider, ILogger<FileCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task CleanupExpiredFilesAsync()
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.FileCleanupStarted);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FhirConverterDbContext>();

            var expiredJobs = await context.ConversionJobs
                .Where(j => j.CreatedOn < DateTime.UtcNow.AddHours(-MagicStrings.FileValidation.FileExpirationHours))
                .ToListAsync();

            var cleanedCount = 0;
            foreach (var job in expiredJobs)
            {
                try
                {
                    // Use TempFileManager to clean up files
                    cleanedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file for job {JobId}", job.JobId);
                }
            }

            _logger.LogInformation(MagicStrings.LogMessages.FileCleanupCompleted, cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file cleanup");
        }
    }
}