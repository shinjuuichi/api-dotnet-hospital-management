using WebAPI.DTOs.Medicine;

namespace WebAPI.Services.Interfaces;

public interface IMedicineService
{
    Task<List<MedicineResponseDto>> GetAllMedicinesAsync();
    Task<MedicineResponseDto> GetMedicineByIdAsync(int id);
    Task<MedicineResponseDto> CreateMedicineAsync(CreateMedicineRequestDto request);
    Task<MedicineResponseDto> UpdateMedicineAsync(int id, UpdateMedicineRequestDto request);
    Task DeleteMedicineAsync(int id);
}