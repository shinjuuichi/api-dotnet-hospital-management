using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;
using WebAPI.Utils.Query;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Manager")]
public class SpecialtyController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialtyController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSpecialties([FromQuery] QueryOptions options)
    {
        var result = await _specialtyService.GetAllAsync(options);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecialty(int id)
    {
        var result = await _specialtyService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyDto createSpecialtyDto)
    {
        var result = await _specialtyService.CreateAsync(createSpecialtyDto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSpecialty(int id, [FromBody] UpdateSpecialtyDto updateSpecialtyDto)
    {
        var result = await _specialtyService.UpdateAsync(id, updateSpecialtyDto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpecialty(int id)
    {
        var result = await _specialtyService.DeleteAsync(id);
        return Ok(result);
    }
}
