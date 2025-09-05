using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{nameof(RoleEnum.Manager)},{nameof(RoleEnum.Doctor)}")]
public class PatientsController(IPatientService _patientService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetPatients()
    {
        var patients = await _patientService.GetAllPatientsAsync();
        return Ok(patients);
    }

    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetPatient(int patientId)
    {
        var patient = await _patientService.GetPatientByIdAsync(patientId);
        return Ok(patient);
    }
}
