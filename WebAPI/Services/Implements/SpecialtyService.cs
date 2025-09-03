using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Specialty;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Services.Implements;

public class SpecialtyService : ISpecialtyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Specialty> _specialtyRepository;

    public SpecialtyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _specialtyRepository = _unitOfWork.Repository<Specialty>();
    }

    public async Task<List<SpecialtyResponseDto>> GetAllSpecialtiesAsync()
    {
        var specialties = await _specialtyRepository
            .GetAllQueryable(new[] { "Doctors" })
            .Select(s => new SpecialtyResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                CreationDate = s.CreationDate ?? DateTime.UtcNow,
                DoctorCount = s.Doctors.Count(d => !d.IsDeleted)
            })
            .ToListAsync();

        return specialties;
    }

    public async Task<SpecialtyResponseDto> GetSpecialtyByIdAsync(int id)
    {
        var specialty = await _specialtyRepository
            .GetAllQueryable(new[] { "Doctors" })
            .Where(s => s.Id == id)
            .Select(s => new SpecialtyResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                CreationDate = s.CreationDate ?? DateTime.UtcNow,
                DoctorCount = s.Doctors.Count(d => !d.IsDeleted)
            })
            .FirstOrDefaultAsync();

        if (specialty == null)
            throw new InvalidOperationException("Specialty not found");

        return specialty;
    }

    public async Task<SpecialtyResponseDto> CreateSpecialtyAsync(CreateSpecialtyRequestDto request)
    {
        // Check if specialty already exists
        var existingSpecialty = await _specialtyRepository
            .GetByConditionAsync(s => s.Name.ToLower() == request.Name.ToLower());

        if (existingSpecialty != null)
            throw new InvalidOperationException("Specialty with this name already exists");

        var specialty = new Specialty
        {
            Name = request.Name
        };

        await _specialtyRepository.AddAsync(specialty);
        await _unitOfWork.SaveChangeAsync();

        return new SpecialtyResponseDto
        {
            Id = specialty.Id,
            Name = specialty.Name,
            CreationDate = specialty.CreationDate ?? DateTime.UtcNow,
            DoctorCount = 0
        };
    }

    public async Task<SpecialtyResponseDto> UpdateSpecialtyAsync(int id, UpdateSpecialtyRequestDto request)
    {
        var specialty = await _specialtyRepository.GetByIdAsync(id);

        if (specialty == null)
            throw new InvalidOperationException("Specialty not found");

        // Check if another specialty with same name exists
        var existingSpecialty = await _specialtyRepository
            .GetByConditionAsync(s => s.Name.ToLower() == request.Name.ToLower() && s.Id != id);

        if (existingSpecialty != null)
            throw new InvalidOperationException("Specialty with this name already exists");

        specialty.Name = request.Name;
        _specialtyRepository.Update(specialty);
        await _unitOfWork.SaveChangeAsync();

        return await GetSpecialtyByIdAsync(id);
    }

    public async Task DeleteSpecialtyAsync(int id)
    {
        var specialty = await _specialtyRepository
            .GetByIdAsync(id, new[] { "Doctors" });

        if (specialty == null)
            throw new InvalidOperationException("Specialty not found");

        // Check if specialty has active doctors
        if (specialty.Doctors.Any(d => !d.IsDeleted))
            throw new InvalidOperationException("Cannot delete specialty with active doctors");

        _specialtyRepository.Remove(specialty);
        await _unitOfWork.SaveChangeAsync();
    }
}