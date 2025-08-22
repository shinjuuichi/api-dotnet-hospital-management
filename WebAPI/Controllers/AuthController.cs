using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return Ok(result);
    }

    [HttpPost("confirm-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmOtp([FromBody] ConfirmOtpDto confirmOtpDto)
    {
        var result = await _authService.ConfirmOtpAsync(confirmOtpDto);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Message);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        return Ok(result);
    }

    [HttpPost("resend-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto resendOtpDto)
    {
        var result = await _authService.ResendOtpAsync(resendOtpDto);
        return Ok(result);
    }
}