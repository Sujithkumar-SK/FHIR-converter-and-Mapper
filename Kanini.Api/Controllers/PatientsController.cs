using Kanini.Application.DTOs.Patients;
using Kanini.Application.Services.Patients;
using Kanini.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kanini.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    [HttpPost("identifiers")]
    public async Task<IActionResult> CreatePatientIdentifier([FromBody] CreatePatientRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "System";
            var result = await _patientService.CreatePatientAsync(request, createdBy);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient identifier");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPatients()
    {
        try
        {
            var result = await _patientService.GetAllPatientsAsync();

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all patients");
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("{globalId}")]
    public async Task<IActionResult> GetPatientByGlobalId(Guid globalId)
    {
        try
        {
            var result = await _patientService.GetPatientByGlobalIdAsync(globalId);

            if (result.IsFailure)
            {
                return NotFound(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by global ID {GlobalId}", globalId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpGet("organization/{organizationId}")]
    public async Task<IActionResult> GetPatientsByOrganization(Guid organizationId)
    {
        try
        {
            var result = await _patientService.GetPatientsByOrganizationAsync(organizationId);

            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patients for organization {OrganizationId}", organizationId);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }
}