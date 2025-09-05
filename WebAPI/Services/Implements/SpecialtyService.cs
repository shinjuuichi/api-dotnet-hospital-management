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
        string[] includes = { "Doctors" };
        var specialties = await _specialtyRepository
            .GetAllQueryable(includes)
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
        string[] includes = { "Doctors" };
        var specialty = await _specialtyRepository
            .GetAllQueryable(includes)
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
        var specialty = await _specialtyRepository.GetByIdAsync(id) ?? throw new InvalidOperationException("Specialty not found");
        specialty.Name = request.Name;
        _specialtyRepository.Update(specialty);
        await _unitOfWork.SaveChangeAsync();

        return await GetSpecialtyByIdAsync(id);
    }

    public async Task DeleteSpecialtyAsync(int id)
    {
        string[] includes = { "Doctors" };
        var specialty = await _specialtyRepository
            .GetByIdAsync(id, includes);

        if (specialty == null)
            throw new InvalidOperationException("Specialty not found");

        if (specialty.Doctors.Any(d => !d.IsDeleted))
            throw new InvalidOperationException("Cannot delete specialty with active doctors");

        _specialtyRepository.HardDelete(specialty);
        await _unitOfWork.SaveChangeAsync();
    }
}