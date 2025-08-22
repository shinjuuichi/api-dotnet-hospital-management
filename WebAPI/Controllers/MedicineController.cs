using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Services.Interfaces;
using WebAPI.Utils.Query;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Manager")]
public class MedicineController : ControllerBase
{
    private readonly IMedicineService _medicineService;

    public MedicineController(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMedicines([FromQuery] QueryOptions options)
    {
        var result = await _medicineService.GetAllAsync(options);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicine(int id)
    {
        var result = await _medicineService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicine([FromBody] CreateMedicineDto createMedicineDto)
    {
        var result = await _medicineService.CreateAsync(createMedicineDto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicine(int id, [FromBody] UpdateMedicineDto updateMedicineDto)
    {
        var result = await _medicineService.UpdateAsync(id, updateMedicineDto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        var result = await _medicineService.DeleteAsync(id);
        return Ok(result);
    }
}
