using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto createPrescriptionDto)
    {
        var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var result = await _prescriptionService.CreateAsync(doctorId, createPrescriptionDto);
        return Ok(result);
    }

    [HttpGet("appointment/{appointmentId}")]
    [Authorize(Roles = "Doctor,Customer,Manager")]
    public async Task<IActionResult> GetPrescriptionByAppointment(int appointmentId)
    {
        var result = await _prescriptionService.GetByAppointmentAsync(appointmentId);
        return Ok(result);
    }
}