using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.Doctor;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/doctors")]
[ApiController]
[Authorize(Roles = nameof(RoleEnum.Manager))]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDoctors([FromQuery] bool includeInactive = false)
    {
        try
        {
            var doctors = await _doctorService.GetAllDoctorsAsync(includeInactive);
            return Ok(doctors);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{doctorId}")]
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

    [HttpPost]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequestDto request)
    {
        try
        {
            var doctor = await _doctorService.CreateDoctorAsync(request);
            return CreatedAtAction(nameof(GetDoctor), new { doctorId = doctor.Id }, doctor);
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

    [HttpPut("{doctorId}")]
    public async Task<IActionResult> UpdateDoctor(int doctorId, [FromBody] UpdateDoctorRequestDto request)
    {
        try
        {
            var doctor = await _doctorService.UpdateDoctorAsync(doctorId, request);
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

    [HttpDelete("{doctorId}")]
    public async Task<IActionResult> DeleteDoctor(int doctorId)
    {
        try
        {
            await _doctorService.DeleteDoctorAsync(doctorId);
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

    [HttpPatch("{doctorId}/activate")]
    public async Task<IActionResult> ActivateDoctor(int doctorId)
    {
        try
        {
            var doctor = await _doctorService.ActivateDoctorAsync(doctorId);
            return Ok(doctor);
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

    [HttpPatch("{doctorId}/deactivate")]
    public async Task<IActionResult> DeactivateDoctor(int doctorId)
    {
        try
        {
            var doctor = await _doctorService.DeactivateDoctorAsync(doctorId);
            return Ok(doctor);
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
