using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;
using WebAPI.DTOs.Auth;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService _authService) : BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(new { message = result });
    }

    [HttpPost("register/confirm")]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationRequestDto request)
    {
        var result = await _authService.ConfirmRegistrationAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token not provided");

        await _authService.LogoutAsync(token);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = result });
    }

    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return Ok(new { message = result });
    }
}