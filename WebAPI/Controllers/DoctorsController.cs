using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;
using WebAPI.DTOs.Doctor;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = nameof(RoleEnum.Manager))]
public class DoctorsController(IDoctorService _doctorService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _doctorService.GetAllDoctorsIncludeDeletedAsync();
        return Ok(doctors);
    }

    [HttpGet("{doctorId}")]
    public async Task<IActionResult> GetDoctor(int doctorId)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
        return Ok(doctor);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequestDto request)
    {
        var doctor = await _doctorService.CreateDoctorAsync(request);
        return Ok(doctor);
    }

    [HttpPut("{doctorId}")]
    public async Task<IActionResult> UpdateDoctor(int doctorId, [FromBody] UpdateDoctorRequestDto request)
    {
        var doctor = await _doctorService.UpdateDoctorAsync(doctorId, request);
        return Ok(doctor);
    }

    [HttpPatch("{doctorId}/activate")]
    public async Task<IActionResult> ActivateDoctor(int doctorId)
    {
        var doctor = await _doctorService.ActivateDoctorAsync(doctorId);
        return Ok(doctor);
    }

    [HttpPatch("{doctorId}/deactivate")]
    public async Task<IActionResult> DeactivateDoctor(int doctorId)
    {
        var doctor = await _doctorService.DeactivateDoctorAsync(doctorId);
        return Ok(doctor);
    }
}
