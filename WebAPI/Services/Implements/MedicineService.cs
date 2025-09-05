using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Medicine;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Services.Implements;

public class MedicineService : IMedicineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Medicine> _medicineRepository;

    public MedicineService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _medicineRepository = _unitOfWork.Repository<Medicine>();
    }

    public async Task<List<MedicineResponseDto>> GetAllMedicinesAsync()
    {
        var medicines = await _medicineRepository
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
        var medicine = await _medicineRepository
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
        var medicine = new Medicine
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        await _medicineRepository.AddAsync(medicine);
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
        var medicine = await _medicineRepository.GetByIdAsync(id);

        if (medicine == null)
            throw new InvalidOperationException("Medicine not found");

        medicine.Name = request.Name;
        medicine.Description = request.Description;
        medicine.Price = request.Price;

        _medicineRepository.Update(medicine);
        await _unitOfWork.SaveChangeAsync();

        return await GetMedicineByIdAsync(id);
    }

    public async Task DeleteMedicineAsync(int id)
    {
        string[] includes = { "PrescriptionDetails" };
        var medicine = await _medicineRepository.GetByIdAsync(id, includes);

        if (medicine == null)
            throw new InvalidOperationException("Medicine not found");

        _medicineRepository.Remove(medicine);
        await _unitOfWork.SaveChangeAsync();
    }
}