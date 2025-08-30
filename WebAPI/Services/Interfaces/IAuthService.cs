using WebAPI.DTOs.Auth;

namespace WebAPI.Services.Interfaces;

public interface IAuthService
{

    Task<string> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> ConfirmRegistrationAsync(ConfirmRegistrationRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task LogoutAsync(string token);
    Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<string> ResetPasswordAsync(ResetPasswordRequestDto request);
}