using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.Medicine;
using WebAPI.Models.Enum;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MedicineController(IMedicineService _medicineService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMedicines()
    {
        var medicines = await _medicineService.GetAllMedicinesAsync();
        return Ok(medicines);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicine(int id)
    {
        var medicine = await _medicineService.GetMedicineByIdAsync(id);
        return Ok(medicine);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> CreateMedicine([FromBody] CreateMedicineRequestDto request)
    {
        var medicine = await _medicineService.CreateMedicineAsync(request);
        return CreatedAtAction(nameof(GetMedicine), new { id = medicine.Id }, medicine);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> UpdateMedicine(int id, [FromBody] UpdateMedicineRequestDto request)
    {
        var medicine = await _medicineService.UpdateMedicineAsync(id, request);
        return Ok(medicine);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleEnum.Manager))]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        await _medicineService.DeleteMedicineAsync(id);
        return NoContent();
    }
}