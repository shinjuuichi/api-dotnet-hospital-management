using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs.Prescription;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IUnitOfWork _unitOfWork;

    public PrescriptionsController(IPrescriptionService prescriptionService, IUnitOfWork unitOfWork)
    {
        _prescriptionService = prescriptionService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("appointments/{appointmentId}/prescriptions")]
    [Authorize(Roles = nameof(RoleEnum.Doctor))]
    public async Task<IActionResult> CreatePrescription(int appointmentId, [FromBody] CreatePrescriptionRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Get doctor by user ID
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetByConditionAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound(new { message = "Doctor profile not found" });

            var prescription = await _prescriptionService.CreatePrescriptionAsync(appointmentId, request, doctor.Id);
            return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, prescription);
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

    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetPrescriptions()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = Enum.Parse<RoleEnum>(User.FindFirst(ClaimTypes.Role)?.Value ?? "Customer");

            var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync(userId, (int)userRole);
            return Ok(prescriptions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("prescriptions/{id}")]
    public async Task<IActionResult> GetPrescription(int id)
    {
        try
        {
            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
            return Ok(prescription);
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