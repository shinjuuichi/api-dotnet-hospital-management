using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs.Prescription;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PrescriptionsController(IPrescriptionService _prescriptionService) : ControllerBase
{
    [HttpPost("appointments/{appointmentId}/prescriptions")]
    [Authorize(Roles = nameof(RoleEnum.Doctor))]
    public async Task<IActionResult> CreatePrescription(int appointmentId, [FromBody] CreatePrescriptionRequestDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var prescription = await _prescriptionService.CreatePrescriptionAsync(appointmentId, request, userId);
        return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, prescription);
    }

    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetPrescriptions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = Enum.Parse<RoleEnum>(User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer");

        var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync(userId, (int)userRole);
        return Ok(prescriptions);
    }

    [HttpGet("prescriptions/{id}")]
    public async Task<IActionResult> GetPrescription(int id)
    {
        var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
        return Ok(prescription);
    }
}