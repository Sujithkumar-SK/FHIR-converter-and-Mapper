using Kanini.Application.Services.Files;
using Kanini.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kanini.Api.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IFileValidationService _fileValidationService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileUploadService fileUploadService,
        IFileValidationService fileValidationService,
        ILogger<FilesController> logger)
    {
        _fileUploadService = fileUploadService;
        _fileValidationService = fileValidationService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] Guid? requestId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uploadedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in token"));
            
            var fileInfo = new Application.DTOs.Files.FileUploadInfo
            {
                FileName = file.FileName,
                Length = file.Length,
                ContentType = file.ContentType,
                Content = file.OpenReadStream()
            };
            
            var result = await _fileUploadService.UploadFileAsync(fileInfo, requestId, uploadedBy, userId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("{fileId}/validate")]
    public async Task<IActionResult> ValidateFile(Guid fileId)
    {
        try
        {
            var result = await _fileValidationService.ValidateFileAsync(fileId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file {FileId}", fileId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("{fileId}/preview")]
    public async Task<IActionResult> GetFilePreview(Guid fileId)
    {
        try
        {
            var result = await _fileUploadService.GetFilePreviewAsync(fileId);

            if (result.IsFailure)
            {
                return NotFound(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file preview {FileId}", fileId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        try
        {
            var result = await _fileUploadService.DeleteFileAsync(fileId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(new { message = MagicStrings.SuccessMessages.FileDeleted });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }
}