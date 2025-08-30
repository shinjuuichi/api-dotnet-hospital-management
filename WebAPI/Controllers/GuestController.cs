using Microsoft.AspNetCore.Mvc;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/guest")]
[ApiController]
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

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors()
    {
        try
        {
            var doctors = await _doctorService.GetAllDoctorsAsync(false); // Only active doctors
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("doctors/{doctorId}")]
    public async Task<IActionResult> GetDoctor(int doctorId)
    {
        try
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            return Ok(doctor);
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
}