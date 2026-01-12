using AutoMapper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Kanini.Application.DTOs.Conversion;
using Kanini.Application.Fhir;
using Kanini.Application.Parsers;
using Kanini.Application.Services.Files;
using Kanini.Common.Constants;
using Kanini.Common.Results;
using Kanini.Data.DatabaseContext;
using Kanini.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Kanini.Application.Services.Conversion;

public class FhirConversionService : IFhirConversionService
{
    private readonly FhirConverterDbContext _context;
    private readonly ITempFileManager _tempFileManager;
    private readonly IFhirConverter _fhirConverter;
    private readonly ICsvParser _csvParser;
    private readonly IJsonParser _jsonParser;
    private readonly ICcdaParser _ccdaParser;
    private readonly IMapper _mapper;
    private readonly ILogger<FhirConversionService> _logger;
    private readonly FhirJsonSerializer _fhirSerializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Guid, List<FieldMappingDto>> _fieldMappingsCache = new();

    public FhirConversionService(
        FhirConverterDbContext context,
        ITempFileManager tempFileManager,
        IFhirConverter fhirConverter,
        ICsvParser csvParser,
        IJsonParser jsonParser,
        ICcdaParser ccdaParser,
        IMapper mapper,
        ILogger<FhirConversionService> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _tempFileManager = tempFileManager;
        _fhirConverter = fhirConverter;
        _csvParser = csvParser;
        _jsonParser = jsonParser;
        _ccdaParser = ccdaParser;
        _mapper = mapper;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _fhirSerializer = new FhirJsonSerializer();
    }

    public async System.Threading.Tasks.Task<Result<ConversionStatusResponseDto>> StartConversionAsync(StartConversionRequestDto request, Guid userId)
    {
        try
        {
            _logger.LogInformation("Starting conversion for FileId: {FileId}", request.FileId);

            // Check if file exists
            if (!_tempFileManager.FileExists(request.FileId))
            {
                return Result.Failure<ConversionStatusResponseDto>(MagicStrings.ErrorMessages.FileExpired);
            }

            // Check if there's already a processing job for this file
            var existingJob = await _context.ConversionJobs
                .FirstOrDefaultAsync(j => j.UserId == userId && 
                                        j.Status == ConversionStatus.Processing &&
                                        j.OriginalFileName != null &&
                                        j.OriginalFileName.Contains(request.FileId.ToString()));

            if (existingJob != null)
            {
                return Result.Failure<ConversionStatusResponseDto>(MagicStrings.ErrorMessages.ConversionInProgress);
            }

            // Validate field mappings
            var validationResult = ValidateFieldMappings(request.FieldMappings);
            if (validationResult.IsFailure)
            {
                return Result.Failure<ConversionStatusResponseDto>(validationResult.Error);
            }

            // Get file info to determine format
            var fileInfo = _tempFileManager.GetFileInfo(request.FileId);
            var inputFormat = DetermineInputFormat(fileInfo.FileName);

            // Get requestId from file mapping if not provided
            var actualRequestId = request.RequestId ?? FileUploadService.GetRequestIdForFile(request.FileId);

            // Create new conversion job
            var job = new Domain.Entities.ConversionJob
            {
                JobId = Guid.NewGuid(),
                UserId = userId,
                RequestId = actualRequestId,
                InputFormat = inputFormat,
                Status = ConversionStatus.Processing,
                OriginalFileName = $"{request.FileId}_{fileInfo.FileName}",
                FileSizeBytes = fileInfo.Size,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };

            _context.ConversionJobs.Add(job);
            await _context.SaveChangesAsync();

            // Cache field mappings for later use
            _fieldMappingsCache[job.JobId] = request.FieldMappings;

            // Start conversion in background with proper DI scope
            _ = System.Threading.Tasks.Task.Run(async () => 
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedContext = scope.ServiceProvider.GetRequiredService<FhirConverterDbContext>();
                
                try
                {
                    _logger.LogInformation("Background conversion task started for JobId: {JobId}", job.JobId);
                    await ProcessConversionAsync(job.JobId, request.FieldMappings, scopedContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background conversion task failed for JobId: {JobId}", job.JobId);
                }
            });

            var response = _mapper.Map<ConversionStatusResponseDto>(job);
            response.Progress = 0;

            _logger.LogInformation("Conversion job created - FileId: {FileId}, JobId: {JobId}", request.FileId, job.JobId);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.ConversionFailed, Guid.Empty, ex.Message);
            return Result.Failure<ConversionStatusResponseDto>(MagicStrings.ErrorMessages.ConversionFailed);
        }
    }

    public async System.Threading.Tasks.Task<Result<ConversionStatusResponseDto>> GetConversionStatusAsync(Guid jobId)
    {
        try
        {
            var job = await _context.ConversionJobs.FindAsync(jobId);
            if (job == null)
            {
                return Result.Failure<ConversionStatusResponseDto>(MagicStrings.ErrorMessages.ConversionJobNotFound);
            }

            var response = _mapper.Map<ConversionStatusResponseDto>(job);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversion status for JobId: {JobId}", jobId);
            return Result.Failure<ConversionStatusResponseDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async System.Threading.Tasks.Task<Result<FhirBundlePreviewDto>> GetFhirPreviewAsync(Guid jobId)
    {
        try
        {
            var job = await _context.ConversionJobs.FindAsync(jobId);
            if (job == null)
            {
                return Result.Failure<FhirBundlePreviewDto>(MagicStrings.ErrorMessages.ConversionJobNotFound);
            }

            if (job.Status != ConversionStatus.Completed)
            {
                return Result.Failure<FhirBundlePreviewDto>("Conversion not completed yet");
            }

            var preview = new FhirBundlePreviewDto
            {
                JobId = jobId,
                BundleId = $"Bundle-{jobId}",
                PatientCount = 1,
                ObservationCount = job.ObservationsCount,
                PatientSample = new List<string> { $"Patient/{job.JobId}" },
                ObservationSample = new List<string> { "Observation/obs-1", "Observation/obs-2" }
            };

            return Result.Success(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FHIR preview for JobId: {JobId}", jobId);
            return Result.Failure<FhirBundlePreviewDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async System.Threading.Tasks.Task<Result<byte[]>> DownloadFhirBundleAsync(Guid jobId)
    {
        try
        {
            var job = await _context.ConversionJobs.FindAsync(jobId);
            if (job == null)
            {
                return Result.Failure<byte[]>(MagicStrings.ErrorMessages.ConversionJobNotFound);
            }

            if (job.Status != ConversionStatus.Completed)
            {
                return Result.Failure<byte[]>("Conversion not completed yet");
            }

            // Generate FHIR bundle using HL7 FHIR package
            var bundleResult = await GenerateFhirBundleAsync(job);
            if (bundleResult.IsFailure)
            {
                return Result.Failure<byte[]>(bundleResult.Error);
            }

            // Serialize to JSON using FHIR serializer
            var bundleJson = _fhirSerializer.SerializeToString(bundleResult.Value);
            var bundleBytes = Encoding.UTF8.GetBytes(bundleJson);

            // Clean up cache after successful download
            _fieldMappingsCache.Remove(jobId);

            // Update DataRequest status if linked
            if (job.RequestId.HasValue)
            {
                var dataRequest = await _context.DataRequests.FindAsync(job.RequestId.Value);
                if (dataRequest != null && dataRequest.Status == DataRequestStatus.DataReady)
                {
                    dataRequest.Status = DataRequestStatus.Completed;
                    dataRequest.UpdatedBy = "System";
                    dataRequest.UpdatedOn = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("DataRequest {RequestId} marked as completed", job.RequestId.Value);
                }
            }

            _logger.LogInformation(MagicStrings.LogMessages.FhirBundleGenerated, jobId, bundleBytes.Length);
            return Result.Success(bundleBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading FHIR bundle for JobId: {JobId}", jobId);
            return Result.Failure<byte[]>(MagicStrings.ErrorMessages.FhirBundleGenerationFailed);
        }
    }

    public async System.Threading.Tasks.Task<Result<IEnumerable<ConversionStatusResponseDto>>> GetConversionHistoryAsync(Guid userId)
    {
        try
        {
            var jobs = await _context.ConversionJobs
                .Where(j => j.UserId == userId || (j.RequestId.HasValue && 
                    _context.DataRequests.Any(dr => dr.RequestId == j.RequestId && dr.RequestingUserId == userId)))
                .OrderByDescending(j => j.CreatedOn)
                .ToListAsync();

            var response = _mapper.Map<IEnumerable<ConversionStatusResponseDto>>(jobs);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversion history for UserId: {UserId}", userId);
            return Result.Failure<IEnumerable<ConversionStatusResponseDto>>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    private async System.Threading.Tasks.Task ProcessConversionAsync(Guid jobId, List<FieldMappingDto> fieldMappings, FhirConverterDbContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting conversion process for JobId: {JobId}", jobId);
            
            // Get fresh job from database
            var job = await context.ConversionJobs.FindAsync(jobId);
            if (job == null)
            {
                _logger.LogError("Job not found for JobId: {JobId}", jobId);
                return;
            }

            // Extract FileId from OriginalFileName and use it to get the existing file
            var fileIdStr = job.OriginalFileName?.Split('_')[0];
            var fileId = Guid.TryParse(fileIdStr, out var parsedFileId) ? parsedFileId : job.JobId;
            
            // Get the actual filename that exists on disk (FileId_OriginalName)
            var filePath = _tempFileManager.GetTempFilePath(fileId, job.OriginalFileName?.Substring(37) ?? "");
            
            _logger.LogInformation("Processing file: {FilePath} for JobId: {JobId}", filePath, jobId);
            
            // Step 1: Parse file to internal models
            var parseResult = job.InputFormat switch
            {
                InputFormat.CSV => await _csvParser.ParseAsync(filePath, fieldMappings, jobId),
                InputFormat.JSON => await _jsonParser.ParseAsync(filePath, fieldMappings, jobId),
                InputFormat.CCDA => await _ccdaParser.ParseAsync(filePath, fieldMappings, jobId),
                _ => Result.Failure<(Models.InternalPatient, List<Models.InternalObservation>)>("Unsupported format")
            };

            if (parseResult.IsFailure)
            {
                throw new Exception(parseResult.Error);
            }

            var (internalPatient, internalObservations) = parseResult.Value;
            _logger.LogInformation("Parsed 1 patient and {ObservationCount} observations for JobId: {JobId}", 
                internalObservations.Count, jobId);

            // Step 2: Convert internal models to FHIR resources
            var patientResult = _fhirConverter.ConvertPatient(internalPatient);
            if (patientResult.IsFailure)
            {
                throw new Exception(patientResult.Error);
            }

            var patient = patientResult.Value;
            var observations = new List<Observation>();

            foreach (var internalObservation in internalObservations)
            {
                var observationResult = _fhirConverter.ConvertObservation(internalObservation);
                if (observationResult.IsSuccess)
                    observations.Add(observationResult.Value);
            }

            // Update job status
            job.Status = ConversionStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            job.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
            job.PatientsCount = 1;
            job.ObservationsCount = observations.Count;
            job.UpdatedBy = "System";
            job.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Update DataRequest status to DataReady if linked
            if (job.RequestId.HasValue)
            {
                var dataRequest = await context.DataRequests.FindAsync(job.RequestId.Value);
                if (dataRequest != null && dataRequest.Status == DataRequestStatus.Approved)
                {
                    dataRequest.Status = DataRequestStatus.DataReady;
                    dataRequest.UpdatedBy = "System";
                    dataRequest.UpdatedOn = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                    _logger.LogInformation("DataRequest {RequestId} marked as DataReady", job.RequestId.Value);
                }
            }

            _logger.LogInformation(MagicStrings.LogMessages.ConversionCompleted, job.JobId, job.PatientsCount, job.ObservationsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion failed for JobId: {JobId}, Error: {Error}", jobId, ex.Message);
            
            // Update job with error
            var job = await context.ConversionJobs.FindAsync(jobId);
            if (job != null)
            {
                job.Status = ConversionStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.UpdatedBy = "System";
                job.UpdatedOn = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
            }
        }
        finally
        {
            stopwatch.Stop();
        }
    }



    private async System.Threading.Tasks.Task<Result<Bundle>> GenerateFhirBundleAsync(Domain.Entities.ConversionJob job)
    {
        try
        {
            // Extract FileId from OriginalFileName and use it to get the existing file
            var fileIdStr = job.OriginalFileName?.Split('_')[0];
            var fileId = Guid.TryParse(fileIdStr, out var parsedFileId) ? parsedFileId : job.JobId;
            
            // Get the actual filename that exists on disk
            var filePath = _tempFileManager.GetTempFilePath(fileId, job.OriginalFileName?.Substring(37) ?? "");
            
            // Get cached field mappings or recreate from field detection
            var fieldMappings = _fieldMappingsCache.GetValueOrDefault(job.JobId, new List<FieldMappingDto>());
            
            if (!fieldMappings.Any())
            {
                // Try to recreate field mappings from file headers
                fieldMappings = await RecreateFieldMappingsAsync(filePath, job.InputFormat);
            }
            
            var parseResult = job.InputFormat switch
            {
                InputFormat.CSV => await _csvParser.ParseAsync(filePath, fieldMappings, job.JobId),
                InputFormat.JSON => await _jsonParser.ParseAsync(filePath, fieldMappings, job.JobId),
                InputFormat.CCDA => await _ccdaParser.ParseAsync(filePath, fieldMappings, job.JobId),
                _ => Result.Failure<(Models.InternalPatient, List<Models.InternalObservation>)>("Unsupported format")
            };

            if (parseResult.IsFailure)
                return Result.Failure<Bundle>(parseResult.Error);

            var (internalPatient, internalObservations) = parseResult.Value;
            
            var patientResult = _fhirConverter.ConvertPatient(internalPatient);
            if (patientResult.IsFailure)
                return Result.Failure<Bundle>(patientResult.Error);

            var patient = patientResult.Value;
            var observations = new List<Observation>();

            foreach (var internalObservation in internalObservations)
            {
                var observationResult = _fhirConverter.ConvertObservation(internalObservation);
                if (observationResult.IsSuccess)
                    observations.Add(observationResult.Value);
            }

            return _fhirConverter.CreateBundle(patient, observations, job.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating FHIR bundle for JobId: {JobId}", job.JobId);
            return Result.Failure<Bundle>("Failed to generate FHIR bundle");
        }
    }

    public async System.Threading.Tasks.Task<Result<ConversionStatusResponseDto>> GetConversionByRequestIdAsync(Guid requestId)
    {
        try
        {
            var job = await _context.ConversionJobs
                .FirstOrDefaultAsync(j => j.RequestId == requestId);
            
            if (job == null)
            {
                return Result.Failure<ConversionStatusResponseDto>("No conversion found for this request");
            }

            var response = _mapper.Map<ConversionStatusResponseDto>(job);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversion by RequestId: {RequestId}", requestId);
            return Result.Failure<ConversionStatusResponseDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async System.Threading.Tasks.Task<Result> ResetConversionJobAsync(Guid jobId, Guid userId)
    {
        try
        {
            var job = await _context.ConversionJobs.FindAsync(jobId);
            if (job == null)
            {
                return Result.Failure(MagicStrings.ErrorMessages.ConversionJobNotFound);
            }

            if (job.UserId != userId)
            {
                return Result.Failure("Not authorized to reset this job");
            }

            // Reset job status
            job.Status = ConversionStatus.Failed;
            job.ErrorMessage = "Job reset by user";
            job.UpdatedBy = "User";
            job.UpdatedOn = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Conversion job {JobId} reset by user {UserId}", jobId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting conversion job {JobId}", jobId);
            return Result.Failure(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    private async Task<List<FieldMappingDto>> RecreateFieldMappingsAsync(string filePath, InputFormat format)
    {
        var fieldMappings = new List<FieldMappingDto>();
        
        if (format == InputFormat.CSV)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length > 0)
            {
                var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
                
                // Map common CSV headers to FHIR fields
                var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["patient_id"] = "patient.identifier",
                    ["first_name"] = "patient.name.given",
                    ["last_name"] = "patient.name.family",
                    ["date_of_birth"] = "patient.birthDate",
                    ["gender"] = "patient.gender",
                    ["phone"] = "patient.telecom.phone",
                    ["email"] = "patient.telecom.email",
                    ["test_name"] = "observation.code",
                    ["test_code"] = "observation.code",
                    ["value"] = "observation.valueQuantity.value",
                    ["test_result"] = "observation.valueQuantity.value",
                    ["unit"] = "observation.valueQuantity.unit",
                    ["date"] = "observation.effectiveDateTime",
                    ["test_date"] = "observation.effectiveDateTime"
                };
                
                foreach (var header in headers)
                {
                    var normalizedHeader = header.ToLowerInvariant().Replace(" ", "_");
                    if (mappings.ContainsKey(normalizedHeader))
                    {
                        fieldMappings.Add(new FieldMappingDto
                        {
                            CsvColumn = header,
                            FhirField = mappings[normalizedHeader],
                            IsRequired = normalizedHeader.Contains("patient_id") || normalizedHeader.Contains("name") || normalizedHeader.Contains("test")
                        });
                    }
                }
            }
        }
        
        return fieldMappings;
    }

    private Result ValidateFieldMappings(List<FieldMappingDto> fieldMappings)
    {
        var requiredFields = new[] { "patient.identifier", "patient.name.given", "patient.name.family" };
        
        foreach (var required in requiredFields)
        {
            if (!fieldMappings.Any(m => m.FhirField == required))
            {
                return Result.Failure($"Required field mapping missing: {required}");
            }
        }

        return Result.Success();
    }

    private Domain.Enums.InputFormat DetermineInputFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => Domain.Enums.InputFormat.CSV,
            ".json" => Domain.Enums.InputFormat.JSON,
            ".xml" => Domain.Enums.InputFormat.CCDA,
            _ => Domain.Enums.InputFormat.CSV
        };
    }
}