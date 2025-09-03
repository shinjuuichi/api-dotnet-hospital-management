using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.Specialty;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SpecialtyController(ISpecialtyService _specialtyService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSpecialties()
    {
        var specialties = await _specialtyService.GetAllSpecialtiesAsync();
        return Ok(specialties);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecialty(int id)
    {
        var specialty = await _specialtyService.GetSpecialtyByIdAsync(id);
        return Ok(specialty);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyRequestDto request)
    {
        var specialty = await _specialtyService.CreateSpecialtyAsync(request);
        return CreatedAtAction(nameof(GetSpecialty), new { id = specialty.Id }, specialty);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> UpdateSpecialty(int id, [FromBody] UpdateSpecialtyRequestDto request)
    {
        var specialty = await _specialtyService.UpdateSpecialtyAsync(id, request);
        return Ok(specialty);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> DeleteSpecialty(int id)
    {
        await _specialtyService.DeleteSpecialtyAsync(id);
        return NoContent();
    }
}