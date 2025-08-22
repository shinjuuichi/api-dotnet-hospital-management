using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;
using WebAPI.Utils.Query;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Manager")]
public class UserManagementController : ControllerBase
{
    private readonly IUserService _userService;

    public UserManagementController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] QueryOptions options)
    {
        var result = await _userService.GetAllAsync(options);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var result = await _userService.CreateAsync(createUserDto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var result = await _userService.UpdateAsync(id, updateUserDto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return Ok(result);
    }
}
