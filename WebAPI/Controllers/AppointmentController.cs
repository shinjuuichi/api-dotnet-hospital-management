using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;
using WebAPI.Utils.Query;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("my-appointments")]
    [Authorize(Roles = "Customer,Doctor")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] QueryOptions options)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "Doctor")
        {
            var doctorResult = await _appointmentService.GetDoctorAppointmentsAsync(userId, options);
            return Ok(doctorResult);
        }

        // For Customer role
        var patientResult = await _appointmentService.ListForCustomerAsync(userId, options);
        return Ok(patientResult);
    }

    [HttpGet]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetAllAppointments([FromQuery] QueryOptions options)
    {
        var result = await _appointmentService.GetAllAsync(options);
        return Ok(result);
    }

    [HttpPost("request")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RequestAppointment([FromBody] RequestAppointmentDto requestAppointmentDto)
    {
        var patientId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var result = await _appointmentService.RequestAsync(patientId, requestAppointmentDto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Customer,Doctor,Manager")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Customer,Doctor,Manager")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole == "Customer")
        {
            var result = await _appointmentService.CancelByCustomerAsync(id, userId);
            return Ok(result);
        }
        else if (userRole == "Doctor")
        {
            var result = await _appointmentService.CancelByDoctorAsync(id, userId);
            return Ok(result);
        }
        
        // For Manager role - they can cancel any appointment
        var managerResult = await _appointmentService.CancelByCustomerAsync(id, userId);
        return Ok(managerResult);
    }

    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> ConfirmAppointment(int id)
    {
        var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var result = await _appointmentService.ConfirmAsync(id, doctorId);
        return Ok(result);
    }

    [HttpPut("{id}/complete")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var result = await _appointmentService.CompleteAsync(id, doctorId);
        return Ok(result);
    }

    [HttpPut("{id}/assign-doctor")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> AssignDoctor(int id, [FromBody] AssignDoctorDto assignDoctorDto)
    {
        var result = await _appointmentService.AssignDoctorAsync(id, assignDoctorDto);
        return Ok(result);
    }
}
