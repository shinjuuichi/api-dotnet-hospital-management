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
        var specialties = await _specialtyService.GetAllSpecialtiesAsync();
        return Ok(specialties);
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync(false); // Only active doctors
        return Ok(doctors);
    }

    [HttpGet("doctors/{doctorId}")]
    public async Task<IActionResult> GetDoctor(int doctorId)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
        return Ok(doctor);
    }
}