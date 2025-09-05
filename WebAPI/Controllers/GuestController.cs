using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GuestController(ISpecialtyService _specialtyService, IDoctorService _doctorService) : BaseController
{
    [HttpGet("specialties")]
    public async Task<IActionResult> GetSpecialties()
    {
        var specialties = await _specialtyService.GetAllSpecialtiesAsync();
        return Ok(specialties);
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return Ok(doctors);
    }

    [HttpGet("doctors/{doctorId}")]
    public async Task<IActionResult> GetDoctor(int doctorId)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
        return Ok(doctor);
    }
}