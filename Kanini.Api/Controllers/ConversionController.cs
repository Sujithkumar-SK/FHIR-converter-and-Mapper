using Kanini.Application.DTOs.Conversion;
using Kanini.Application.Services.Conversion;
using Kanini.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kanini.Api.Controllers;

[ApiController]
[Route("api/convert")]
[Authorize]
public class ConversionController : ControllerBase
{
    private readonly IFhirConversionService _conversionService;
    private readonly IFieldDetectionService _fieldDetectionService;
    private readonly ILogger<ConversionController> _logger;

    public ConversionController(
        IFhirConversionService conversionService,
        IFieldDetectionService fieldDetectionService,
        ILogger<ConversionController> logger)
    {
        _conversionService = conversionService;
        _fieldDetectionService = fieldDetectionService;
        _logger = logger;
    }

    [HttpGet("{fileId}/detect-fields")]
    public async Task<IActionResult> DetectFields(Guid fileId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _fieldDetectionService.DetectFieldsAsync(fileId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting fields for FileId: {FileId}", fileId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("fhir-fields")]
    public async Task<IActionResult> GetAvailableFhirFields()
    {
        try
        {
            var result = await _fieldDetectionService.GetAvailableFhirFieldsAsync();

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available FHIR fields");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartConversion([FromBody] StartConversionRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            var result = await _conversionService.StartConversionAsync(request, userId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting conversion for FileId: {FileId}", request.FileId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("status/{jobId}")]
    public async Task<IActionResult> GetConversionStatus(Guid jobId)
    {
        try
        {
            var result = await _conversionService.GetConversionStatusAsync(jobId);

            if (result.IsFailure)
            {
                return NotFound(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversion status for JobId: {JobId}", jobId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("preview/{jobId}")]
    public async Task<IActionResult> GetFhirPreview(Guid jobId)
    {
        try
        {
            var result = await _conversionService.GetFhirPreviewAsync(jobId);

            if (result.IsFailure)
            {
                return NotFound(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FHIR preview for JobId: {JobId}", jobId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("download/{jobId}")]
    public async Task<IActionResult> DownloadFhirBundle(Guid jobId)
    {
        try
        {
            var result = await _conversionService.DownloadFhirBundleAsync(jobId);

            if (result.IsFailure)
            {
                return NotFound(new { message = result.Error });
            }

            var fileName = $"fhir-bundle-{jobId}.json";
            return File(result.Value, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading FHIR bundle for JobId: {JobId}", jobId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpPost("reset/{jobId}")]
    public async Task<IActionResult> ResetConversionJob(Guid jobId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            var result = await _conversionService.ResetConversionJobAsync(jobId, userId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(new { message = "Conversion job reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting conversion job {JobId}", jobId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("by-request/{requestId}")]
    public async Task<IActionResult> GetConversionByRequestId(Guid requestId)
    {
        try
        {
            var result = await _conversionService.GetConversionByRequestIdAsync(requestId);

            if (result.IsFailure)
            {
                return NotFound(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversion by RequestId: {RequestId}", requestId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetConversionHistory()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            var result = await _conversionService.GetConversionHistoryAsync(userId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversion history");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }
}