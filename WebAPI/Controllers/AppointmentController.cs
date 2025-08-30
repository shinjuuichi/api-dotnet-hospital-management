using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs.Appointment;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/appointments")]
[ApiController]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentController(IAppointmentService appointmentService, IUnitOfWork unitOfWork)
    {
        _appointmentService = appointmentService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = Enum.Parse<RoleEnum>(User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer");

            var appointments = await _appointmentService.GetAllAppointmentsAsync(userId, (int)userRole);
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{appointmentId}")]
    public async Task<IActionResult> GetAppointment(int appointmentId)
    {
        try
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            return Ok(appointment);
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
    [Authorize(Roles = nameof(RoleEnum.Customer))]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var patient = await _unitOfWork.Repository<Patient>()
                .GetByConditionAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound(new { message = "Patient profile not found" });

            var appointment = await _appointmentService.CreateAppointmentAsync(patient.Id, request);
            return CreatedAtAction(nameof(GetAppointment), new { appointmentId = appointment.Id }, appointment);
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

    [HttpDelete("{appointmentId}")]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = Enum.Parse<RoleEnum>(User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer");

            await _appointmentService.DeleteAppointmentAsync(appointmentId, userId, (int)userRole);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{appointmentId}/confirm")]
    [Authorize(Roles = $"{nameof(RoleEnum.Doctor)},{nameof(RoleEnum.Manager)}")]
    public async Task<IActionResult> ConfirmAppointment(int appointmentId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appointment = await _appointmentService.ConfirmAppointmentAsync(appointmentId, userId);
            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{appointmentId}/cancel")]
    public async Task<IActionResult> CancelAppointment(int appointmentId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = Enum.Parse<RoleEnum>(User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer");

            var appointment = await _appointmentService.CancelAppointmentAsync(appointmentId, userId, (int)userRole);
            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{appointmentId}/complete")]
    [Authorize(Roles = nameof(RoleEnum.Doctor))]
    public async Task<IActionResult> CompleteAppointment(int appointmentId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appointment = await _appointmentService.CompleteAppointmentAsync(appointmentId, userId);
            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{appointmentId}/assign-doctor")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> AssignDoctor(int appointmentId, [FromBody] AssignDoctorRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appointment = await _appointmentService.AssignDoctorAsync(appointmentId, request, userId);
            return Ok(appointment);
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