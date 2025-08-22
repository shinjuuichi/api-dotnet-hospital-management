using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Implements;

public class SpecialtyService : ISpecialtyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SpecialtyService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<SpecialtyDto>>> GetAllAsync(QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Specialty>().GetAllQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(s => 
                    s.Name.Contains(options.Search) ||
                    (s.Description != null && s.Description.Contains(options.Search)));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "description" => isDescending ? query.OrderByDescending(s => s.Description) : query.OrderBy(s => s.Description),
                "createdat" => isDescending ? query.OrderByDescending(s => s.CreationDate) : query.OrderBy(s => s.CreationDate),
                _ => query.OrderBy(s => s.Name)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<SpecialtyDto>);
            return Result<PagedResult<SpecialtyDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<SpecialtyDto>>.Failure($"Error retrieving specialties: {ex.Message}");
        }
    }

    public async Task<Result<SpecialtyDto>> GetByIdAsync(int id)
    {
        try
        {
            var specialty = await _unitOfWork.Repository<Specialty>()
                .GetAllQueryable(new[] { "Doctors" })
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
                return Result<SpecialtyDto>.Failure("Specialty not found");

            var specialtyDto = _mapper.Map<SpecialtyDto>(specialty);
            return Result<SpecialtyDto>.Success(specialtyDto);
        }
        catch (Exception ex)
        {
            return Result<SpecialtyDto>.Failure($"Error retrieving specialty: {ex.Message}");
        }
    }

    public async Task<Result<SpecialtyDto>> CreateAsync(CreateSpecialtyDto createSpecialtyDto)
    {
        try
        {
            // Check if specialty with same name already exists
            var existingSpecialty = await _unitOfWork.Repository<Specialty>()
                .GetByConditionAsync(s => s.Name.ToLower() == createSpecialtyDto.Name.ToLower());

            if (existingSpecialty != null)
                return Result<SpecialtyDto>.Failure("Specialty with this name already exists");

            var specialty = _mapper.Map<Specialty>(createSpecialtyDto);

            await _unitOfWork.Repository<Specialty>().AddAsync(specialty);
            await _unitOfWork.SaveChangeAsync();

            var specialtyDto = _mapper.Map<SpecialtyDto>(specialty);
            return Result<SpecialtyDto>.Success(specialtyDto);
        }
        catch (Exception ex)
        {
            return Result<SpecialtyDto>.Failure($"Error creating specialty: {ex.Message}");
        }
    }

    public async Task<Result<SpecialtyDto>> UpdateAsync(int id, UpdateSpecialtyDto updateSpecialtyDto)
    {
        try
        {
            var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(id);
            if (specialty == null)
                return Result<SpecialtyDto>.Failure("Specialty not found");

            // Check if new name already exists (excluding current specialty)
            if (!string.IsNullOrEmpty(updateSpecialtyDto.Name) && 
                updateSpecialtyDto.Name.ToLower() != specialty.Name.ToLower())
            {
                var existingSpecialty = await _unitOfWork.Repository<Specialty>()
                    .GetByConditionAsync(s => s.Name.ToLower() == updateSpecialtyDto.Name.ToLower());

                if (existingSpecialty != null)
                    return Result<SpecialtyDto>.Failure("Specialty with this name already exists");
            }

            _mapper.Map(updateSpecialtyDto, specialty);

            _unitOfWork.Repository<Specialty>().Update(specialty);
            await _unitOfWork.SaveChangeAsync();

            var specialtyDto = _mapper.Map<SpecialtyDto>(specialty);
            return Result<SpecialtyDto>.Success(specialtyDto);
        }
        catch (Exception ex)
        {
            return Result<SpecialtyDto>.Failure($"Error updating specialty: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var specialty = await _unitOfWork.Repository<Specialty>()
                .GetAllQueryable(new[] { "Doctors" })
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
                return Result.Failure("Specialty not found");

            // Check if specialty has associated doctors
            if (specialty.Doctors.Any())
                return Result.Failure("Cannot delete specialty that has associated doctors");

            _unitOfWork.Repository<Specialty>().Remove(specialty);
            await _unitOfWork.SaveChangeAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting specialty: {ex.Message}");
        }
    }
}
