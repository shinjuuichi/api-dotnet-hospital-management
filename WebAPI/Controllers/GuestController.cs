using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;
using WebAPI.Utils.Query;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class GuestController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;
    private readonly IDoctorService _doctorService;

    public GuestController(ISpecialtyService specialtyService, IDoctorService doctorService)
    {
        _specialtyService = specialtyService;
        _doctorService = doctorService;
    }

    [HttpGet("specialties")]
    public async Task<IActionResult> GetSpecialties()
    {
        var queryOptions = new QueryOptions { Page = 1, PageSize = int.MaxValue };
        var result = await _specialtyService.GetAllAsync(queryOptions);
        return Ok(result);
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors([FromQuery] int? specialtyId)
    {
        var queryOptions = new QueryOptions { Page = 1, PageSize = int.MaxValue };
        
        if (specialtyId.HasValue)
        {
            var result = await _doctorService.GetBySpecialtyAsync(specialtyId.Value, queryOptions);
            return Ok(result);
        }
        else
        {
            var result = await _doctorService.GetAllAsync(queryOptions);
            return Ok(result);
        }
    }

    [HttpGet("doctors/{id}")]
    public async Task<IActionResult> GetDoctor(int id)
    {
        var result = await _doctorService.GetByIdAsync(id);
        return Ok(result);
    }
}
