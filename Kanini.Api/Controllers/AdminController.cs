using Kanini.Application.Services.Analytics;
using Kanini.Common.Constants;
using Kanini.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Kanini.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAnalyticsService analyticsService, ILogger<AdminController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetSystemOverview()
    {
        try
        {
            if (!IsAdminUser())
            {
                return Forbid(MagicStrings.ErrorMessages.UnauthorizedAnalyticsAccess);
            }

            var result = await _analyticsService.GetSystemOverviewAsync();

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system overview");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("conversions/statistics")]
    public async Task<IActionResult> GetConversionStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            if (!IsAdminUser())
            {
                return Forbid(MagicStrings.ErrorMessages.UnauthorizedAnalyticsAccess);
            }

            if (!ValidateDateRange(startDate, endDate))
            {
                return BadRequest(new { message = MagicStrings.ErrorMessages.InvalidDateRange });
            }

            var result = await _analyticsService.GetConversionStatisticsAsync(startDate, endDate);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversion statistics");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("users/activity")]
    public async Task<IActionResult> GetUserActivityStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            if (!IsAdminUser())
            {
                return Forbid(MagicStrings.ErrorMessages.UnauthorizedAnalyticsAccess);
            }

            if (!ValidateDateRange(startDate, endDate))
            {
                return BadRequest(new { message = MagicStrings.ErrorMessages.InvalidDateRange });
            }

            var result = await _analyticsService.GetUserActivityStatsAsync(startDate, endDate);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity statistics");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("data-requests/statistics")]
    public async Task<IActionResult> GetDataRequestStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            if (!IsAdminUser())
            {
                return Forbid(MagicStrings.ErrorMessages.UnauthorizedAnalyticsAccess);
            }

            if (!ValidateDateRange(startDate, endDate))
            {
                return BadRequest(new { message = MagicStrings.ErrorMessages.InvalidDateRange });
            }

            var result = await _analyticsService.GetDataRequestStatsAsync(startDate, endDate);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data request statistics");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("organizations/statistics")]
    public async Task<IActionResult> GetOrganizationStats()
    {
        try
        {
            if (!IsAdminUser())
            {
                return Forbid(MagicStrings.ErrorMessages.UnauthorizedAnalyticsAccess);
            }

            var result = await _analyticsService.GetOrganizationStatsAsync();

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization statistics");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    private bool IsAdminUser()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        return userRole == UserRole.Admin.ToString();
    }

    private bool ValidateDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate > endDate)
                return false;
            
            if (startDate > DateTime.UtcNow)
                return false;
                
            if ((endDate - startDate)?.TotalDays > 365)
                return false;
        }
        
        return true;
    }
}