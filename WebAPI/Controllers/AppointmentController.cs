using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Controllers.Base;
using WebAPI.DTOs.Appointment;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppointmentController(IAppointmentService _appointmentService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = Enum.Parse<RoleEnum>(User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer");

        var appointments = await _appointmentService.GetAllAppointmentsAsync(userId, (int)userRole);
        return Ok(appointments);
    }

    [HttpGet("{appointmentId}")]
    public async Task<IActionResult> GetAppointment(int appointmentId)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        return Ok(appointment);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleEnum.Customer))]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var appointment = await _appointmentService.CreateAppointmentAsync(userId, request);
        return Ok(appointment);
    }

    [HttpPatch("{appointmentId}/confirm")]
    [Authorize(Roles = $"{nameof(RoleEnum.Doctor)},{nameof(RoleEnum.Manager)}")]
    public async Task<IActionResult> ConfirmAppointment(int appointmentId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var appointment = await _appointmentService.ConfirmAppointmentAsync(appointmentId, userId);
        return Ok(appointment);
    }

    [HttpPatch("{appointmentId}/cancel")]
    public async Task<IActionResult> CancelAppointment(int appointmentId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = Enum.Parse<RoleEnum>(User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer");

        var appointment = await _appointmentService.CancelAppointmentAsync(appointmentId, userId, (int)userRole);
        return Ok(appointment);
    }

    [HttpPatch("{appointmentId}/complete")]
    [Authorize(Roles = nameof(RoleEnum.Doctor))]
    public async Task<IActionResult> CompleteAppointment(int appointmentId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var appointment = await _appointmentService.CompleteAppointmentAsync(appointmentId, userId);
        return Ok(appointment);
    }

    [HttpPatch("{appointmentId}/assign-doctor")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> AssignDoctor(int appointmentId, [FromBody] AssignDoctorRequestDto request)
    {
        var appointment = await _appointmentService.AssignDoctorAsync(appointmentId, request);
        return Ok(appointment);
    }
}