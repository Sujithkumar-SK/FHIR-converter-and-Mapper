using Kanini.Application.DTOs.Auth;
using Kanini.Common.Results;

namespace Kanini.Application.Services.Users;

public interface IAuthService
{
    Task<Result<OtpResponseDto>> RegisterWithOtpAsync(RegisterWithOtpRequestDto request);
    Task<Result<RegistrationResponseDto>> VerifyRegistrationOtpAsync(VerifyRegistrationOtpRequestDto request);
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
}