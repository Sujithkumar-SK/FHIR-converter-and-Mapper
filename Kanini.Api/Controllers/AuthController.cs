using Kanini.Application.DTOs.Auth;
using Kanini.Application.Services.Users;
using Kanini.Common.Constants;
using Kanini.Data.Repositories.Organizations;
using Kanini.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kanini.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IJwtService jwtService, IOrganizationRepository organizationRepository, IEncryptionService encryptionService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtService = jwtService;
        _organizationRepository = organizationRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    [HttpPost("register-with-otp")]
    public async Task<IActionResult> RegisterWithOtp([FromBody] RegisterWithOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterWithOtpAsync(request);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RegisterWithOtp for email: {Email}", request.Email);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpPost("verify-registration-otp")]
    public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyRegistrationOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.VerifyRegistrationOtpAsync(request);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in VerifyRegistrationOtp for email: {Email}", request.Email);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpPost("resend-registration-otp")]
    public async Task<IActionResult> ResendRegistrationOtp([FromBody] RegisterWithOtpRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterWithOtpAsync(request);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ResendRegistrationOtp for email: {Email}", request.Email);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);
            
            if (result.IsFailure)
            {
                return BadRequest(new { message = result.Error });
            }

            // Get user for JWT token generation
            var user = new Domain.Entities.User
            {
                UserId = Guid.Parse(result.Value.UserId.ToString()),
                Email = result.Value.Email,
                Role = Enum.Parse<Domain.Enums.UserRole>(result.Value.Role),
                OrganizationId = result.Value.OrganizationId // ✅ Add this line
            };

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Set JWT token in HTTP-only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // ✅ Changed to false for HTTP development
                SameSite = SameSiteMode.Lax, // ✅ Changed to Lax for cross-origin
                Expires = DateTime.UtcNow.AddHours(24)
            };
            Response.Cookies.Append("jwt", token, cookieOptions);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Login for email: {Email}", request.Email);
            return StatusCode(500, new { message = MagicStrings.ErrorMessages.InternalServerError });
        }
    }
}