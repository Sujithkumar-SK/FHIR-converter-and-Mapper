using AutoMapper;
using Kanini.Application.DTOs.Auth;
using Kanini.Application.Services.Users;
using Kanini.Common.Constants;
using Kanini.Common.Results;
using Kanini.Data.Repositories.Users;
using Kanini.Data.Repositories.Organizations;
using Kanini.Domain.Entities;
using Kanini.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Kanini.Application.Services.Users;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IEmailService _emailService;
    private readonly IOTPService _otpService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IUserReadRepository userReadRepository,
        IOrganizationRepository organizationRepository,
        IEmailService emailService,
        IOTPService otpService,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _userReadRepository = userReadRepository;
        _organizationRepository = organizationRepository;
        _emailService = emailService;
        _otpService = otpService;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OtpResponseDto>> RegisterWithOtpAsync(RegisterWithOtpRequestDto request)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.RegistrationOtpStarted, request.Email);

            // Check if user already exists
            var userExists = await _userReadRepository.ExistsByEmailAsync(request.Email);
            if (userExists)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                return Result.Failure<OtpResponseDto>(MagicStrings.ErrorMessages.EmailAlreadyExists);
            }

            // Generate OTP
            var otp = _otpService.GenerateOTP();
            
            // Create OTP token with registration data
            var registrationData = System.Text.Json.JsonSerializer.Serialize(request);
            var otpToken = _otpService.CreateOtpToken(request.Email, otp, registrationData);

            // Send OTP email
            var emailBody = MagicStrings.EmailTemplates.GetVerificationCodeEmail(request.Email, otp);
            await _emailService.SendEmailAsync(request.Email, "FHIR Converter - Email Verification", emailBody);

            var response = new OtpResponseDto
            {
                Email = request.Email,
                OtpToken = otpToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Message = MagicStrings.SuccessMessages.OtpSent
            };

            _logger.LogInformation(MagicStrings.LogMessages.RegistrationOtpCompleted, request.Email);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.RegistrationOtpFailed, request.Email, ex.Message);
            return Result.Failure<OtpResponseDto>(MagicStrings.ErrorMessages.InternalServerError);
        }
    }

    public async Task<Result<RegistrationResponseDto>> VerifyRegistrationOtpAsync(VerifyRegistrationOtpRequestDto request)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.RegistrationVerificationStarted, request.Email);

            // Validate OTP token
            var isValidOtp = _otpService.ValidateOtpToken(request.OtpToken, request.Otp, request.Email);
            if (!isValidOtp)
            {
                _logger.LogWarning("Invalid OTP verification attempt for email: {Email}", request.Email);
                return Result.Failure<RegistrationResponseDto>(MagicStrings.ErrorMessages.InvalidOrExpiredOtp);
            }

            // Extract registration data from token
            var registrationData = ExtractRegistrationDataFromToken(request.OtpToken);
            if (registrationData == null)
            {
                return Result.Failure<RegistrationResponseDto>(MagicStrings.ErrorMessages.InvalidOrExpiredOtp);
            }

            // Generate organization ID as GUID
            var organizationId = Guid.NewGuid();

            // Create organization first
            var organization = new Organization
            {
                OrganizationId = organizationId,
                Name = registrationData.OrganizationName,
                Type = registrationData.Role == UserRole.Hospital ? OrganizationType.Hospital : OrganizationType.Clinic,
                ContactEmail = request.Email,
                ContactPhone = registrationData.MobileNumber,
                IsActive = true,
                CreatedBy = request.Email,
                CreatedOn = DateTime.UtcNow
            };

            var createdOrganization = await _organizationRepository.CreateAsync(organization);

            // Create user with proper data
            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashPassword(registrationData.Password),
                Role = request.Role,
                OrganizationId = request.Role == UserRole.Admin ? null : organizationId,
                IsActive = true,
                CreatedBy = request.Email,
                CreatedOn = DateTime.UtcNow
            };

            // Create user
            var createdUser = await _userRepository.CreateAsync(user);

            // Map to response
            var response = _mapper.Map<RegistrationResponseDto>(createdUser);
            response.Message = MagicStrings.SuccessMessages.RegistrationCompleted;

            _logger.LogInformation(MagicStrings.LogMessages.RegistrationVerificationCompleted, request.Email, createdUser.UserId);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.RegistrationVerificationFailed, request.Email, ex.Message);
            return Result.Failure<RegistrationResponseDto>(MagicStrings.ErrorMessages.RegistrationFailed);
        }
    }

    private RegisterWithOtpRequestDto? ExtractRegistrationDataFromToken(string otpToken)
    {
        try
        {
            var tokenBytes = Convert.FromBase64String(otpToken);
            var tokenData = Encoding.UTF8.GetString(tokenBytes);
            var parts = tokenData.Split('|');

            if (parts.Length >= 4)
            {
                var registrationDataJson = parts[3];
                return System.Text.Json.JsonSerializer.Deserialize<RegisterWithOtpRequestDto>(registrationDataJson);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation(MagicStrings.LogMessages.LoginAttemptStarted, request.Email);

            var user = await _userReadRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return Result.Failure<LoginResponseDto>(MagicStrings.ErrorMessages.InvalidCredentials);
            }

            if (!user.IsActive)
            {
                return Result.Failure<LoginResponseDto>(MagicStrings.ErrorMessages.AccountInactive);
            }

            var hashedPassword = HashPassword(request.Password);
            if (user.PasswordHash != hashedPassword)
            {
                return Result.Failure<LoginResponseDto>(MagicStrings.ErrorMessages.InvalidCredentials);
            }

            // Create a clean user entity for update (without navigation properties)
            var userForUpdate = new User
            {
                UserId = user.UserId,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                OrganizationId = user.OrganizationId,
                IsActive = user.IsActive,
                LastLogin = DateTime.UtcNow,
                CreatedBy = user.CreatedBy,
                CreatedOn = user.CreatedOn,
                UpdatedBy = user.UpdatedBy,
                UpdatedOn = user.UpdatedOn
            };

            await _userRepository.UpdateAsync(userForUpdate);

            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role.ToString(),
                OrganizationName = user.Organization?.Name ?? "",
                OrganizationId = user.OrganizationId,
                Message = MagicStrings.SuccessMessages.LoginSuccessful
            };

            _logger.LogInformation(MagicStrings.LogMessages.LoginAttemptCompleted, request.Email, user.UserId);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MagicStrings.LogMessages.LoginAttemptFailed, request.Email, ex.Message);
            return Result.Failure<LoginResponseDto>(MagicStrings.ErrorMessages.LoginFailed);
        }
    }
}