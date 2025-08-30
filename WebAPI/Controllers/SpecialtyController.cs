using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs.Specialty;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/specialties")]
[ApiController]
[Authorize]
public class SpecialtyController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialtyController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSpecialties()
    {
        try
        {
            var specialties = await _specialtyService.GetAllSpecialtiesAsync();
            return Ok(specialties);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecialty(int id)
    {
        try
        {
            var specialty = await _specialtyService.GetSpecialtyByIdAsync(id);
            return Ok(specialty);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyRequestDto request)
    {
        try
        {
            var specialty = await _specialtyService.CreateSpecialtyAsync(request);
            return CreatedAtAction(nameof(GetSpecialty), new { id = specialty.Id }, specialty);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> UpdateSpecialty(int id, [FromBody] UpdateSpecialtyRequestDto request)
    {
        try
        {
            var specialty = await _specialtyService.UpdateSpecialtyAsync(id, request);
            return Ok(specialty);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> DeleteSpecialty(int id)
    {
        try
        {
            await _specialtyService.DeleteSpecialtyAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}