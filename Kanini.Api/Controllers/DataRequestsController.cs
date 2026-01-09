using Kanini.Application.DTOs.DataRequests;
using Kanini.Application.Services.DataRequests;
using Kanini.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kanini.Api.Controllers;

[ApiController]
[Route("api/data-requests")]
[Authorize]
public class DataRequestsController : ControllerBase
{
    private readonly IDataRequestService _dataRequestService;
    private readonly ILogger<DataRequestsController> _logger;

    public DataRequestsController(IDataRequestService dataRequestService, ILogger<DataRequestsController> logger)
    {
        _dataRequestService = dataRequestService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateDataRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestingUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var requestingOrganizationId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? "");
            var createdBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

            var result = await _dataRequestService.CreateRequestAsync(request, requestingUserId, requestingOrganizationId, createdBy);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data request");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequests([FromQuery] bool isRequesting = true)
    {
        try
        {
            var organizationId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? "");

            var result = await _dataRequestService.GetRequestsByOrganizationAsync(organizationId, isRequesting);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data requests");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequest(Guid id)
    {
        try
        {
            var result = await _dataRequestService.GetRequestByIdAsync(id);

            if (result.IsFailure)
            {
                return NotFound(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data request {RequestId}", id);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ApproveDataRequestDto approval)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var approvedByUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            var updatedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";

            var result = await _dataRequestService.ApproveRequestAsync(id, approval, approvedByUserId, updatedBy);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving data request {RequestId}", id);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        try
        {
            var organizationId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? "");

            var result = await _dataRequestService.GetPendingRequestsAsync(organizationId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending data requests");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }
}