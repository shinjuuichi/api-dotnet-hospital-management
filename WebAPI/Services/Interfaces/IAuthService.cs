using WebAPI.DTOs;
using WebAPI.Utils;

namespace WebAPI.Services.Interfaces;

public interface IAuthService
{
    Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<Result> ConfirmOtpAsync(ConfirmOtpDto confirmOtpDto);
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    Task<Result<RegisterResponseDto>> ResendOtpAsync(ResendOtpDto resendOtpDto);
}
