using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Medicine;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Services.Implements;

public class MedicineService : IMedicineService
{
    private readonly IUnitOfWork _unitOfWork;

    public MedicineService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<MedicineResponseDto>> GetAllMedicinesAsync()
    {
        var medicines = await _unitOfWork.Repository<Medicine>()
            .GetAllQueryable()
            .Select(m => new MedicineResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                CreationDate = m.CreationDate ?? DateTime.UtcNow
            })
            .ToListAsync();

        return medicines;
    }

    public async Task<MedicineResponseDto> GetMedicineByIdAsync(int id)
    {
        var medicine = await _unitOfWork.Repository<Medicine>()
            .GetAllQueryable()
            .Where(m => m.Id == id)
            .Select(m => new MedicineResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                CreationDate = m.CreationDate ?? DateTime.UtcNow
            })
            .FirstOrDefaultAsync();

        if (medicine == null)
            throw new InvalidOperationException("Medicine not found");

        return medicine;
    }

    public async Task<MedicineResponseDto> CreateMedicineAsync(CreateMedicineRequestDto request)
    {
        // Check if medicine with same name already exists
        var existingMedicine = await _unitOfWork.Repository<Medicine>()
            .GetByConditionAsync(m => m.Name.ToLower() == request.Name.ToLower());

        if (existingMedicine != null)
            throw new InvalidOperationException("Medicine with this name already exists");

        var medicine = new Medicine
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        await _unitOfWork.Repository<Medicine>().AddAsync(medicine);
        await _unitOfWork.SaveChangeAsync();

        return new MedicineResponseDto
        {
            Id = medicine.Id,
            Name = medicine.Name,
            Description = medicine.Description,
            Price = medicine.Price,
            CreationDate = medicine.CreationDate ?? DateTime.UtcNow
        };
    }

    public async Task<MedicineResponseDto> UpdateMedicineAsync(int id, UpdateMedicineRequestDto request)
    {
        var medicine = await _unitOfWork.Repository<Medicine>().GetByIdAsync(id);

        if (medicine == null)
            throw new InvalidOperationException("Medicine not found");

        // Check if another medicine with same name exists
        var existingMedicine = await _unitOfWork.Repository<Medicine>()
            .GetByConditionAsync(m => m.Name.ToLower() == request.Name.ToLower() && m.Id != id);

        if (existingMedicine != null)
            throw new InvalidOperationException("Medicine with this name already exists");

        medicine.Name = request.Name;
        medicine.Description = request.Description;
        medicine.Price = request.Price;

        _unitOfWork.Repository<Medicine>().Update(medicine);
        await _unitOfWork.SaveChangeAsync();

        return await GetMedicineByIdAsync(id);
    }

    public async Task DeleteMedicineAsync(int id)
    {
        var medicine = await _unitOfWork.Repository<Medicine>()
            .GetByIdAsync(id, new[] { "PrescriptionDetails" });

        if (medicine == null)
            throw new InvalidOperationException("Medicine not found");

        // Check if medicine is used in any prescriptions
        if (medicine.PrescriptionDetails.Any())
            throw new InvalidOperationException("Cannot delete medicine that has been prescribed");

        _unitOfWork.Repository<Medicine>().Remove(medicine);
        await _unitOfWork.SaveChangeAsync();
    }
}