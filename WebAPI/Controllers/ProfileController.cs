using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IDoctorService _doctorService;

    public ProfileController(IUserService userService, IDoctorService doctorService)
    {
        _userService = userService;
        _doctorService = doctorService;
    }

    [HttpGet]
    [Authorize(Roles = "Customer,Doctor,Manager")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "Doctor")
        {
            var doctorResult = await _doctorService.GetByUserIdAsync(userId);
            return Ok(doctorResult);
        }

        var userResult = await _userService.GetByIdAsync(userId);
        return Ok(userResult);
    }

    [HttpPut]
    [Authorize(Roles = "Customer,Doctor,Manager")]
    public async Task<IActionResult> UpdateProfile([FromBody] object profileData)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "Doctor")
        {
            try
            {
                var updateDoctorDto = System.Text.Json.JsonSerializer.Deserialize<UpdateDoctorDto>(
                    System.Text.Json.JsonSerializer.Serialize(profileData));
                
                var result = await _doctorService.UpdateAsync(userId, updateDoctorDto!);
                return Ok(result);
            }
            catch
            {
                return BadRequest("Invalid doctor profile data");
            }
        }

        try
        {
            var updateUserDto = System.Text.Json.JsonSerializer.Deserialize<UpdateUserDto>(
                System.Text.Json.JsonSerializer.Serialize(profileData));
            
            var userResult = await _userService.UpdateAsync(userId, updateUserDto!);
            return Ok(userResult);
        }
        catch
        {
            return BadRequest("Invalid user profile data");
        }
    }
}
