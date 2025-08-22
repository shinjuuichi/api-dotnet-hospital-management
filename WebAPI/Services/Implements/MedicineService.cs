using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Implements;

public class MedicineService : IMedicineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MedicineService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<MedicineDto>>> GetAllAsync(QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Medicine>().GetAllQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(m =>
                    m.Name.Contains(options.Search) ||
                    (m.Description != null && m.Description.Contains(options.Search)) ||
                    (m.Dosage != null && m.Dosage.Contains(options.Search)));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
                "dosage" => isDescending ? query.OrderByDescending(m => m.Dosage) : query.OrderBy(m => m.Dosage),
                "createdat" => isDescending ? query.OrderByDescending(m => m.CreationDate) : query.OrderBy(m => m.CreationDate),
                _ => query.OrderBy(m => m.Name)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<MedicineDto>);
            return Result<PagedResult<MedicineDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<MedicineDto>>.Failure($"Error retrieving medicines: {ex.Message}");
        }
    }

    public async Task<Result<MedicineDto>> GetByIdAsync(int id)
    {
        try
        {
            var medicine = await _unitOfWork.Repository<Medicine>().GetByIdAsync(id);

            if (medicine == null)
                return Result<MedicineDto>.Failure("Medicine not found");

            var medicineDto = _mapper.Map<MedicineDto>(medicine);
            return Result<MedicineDto>.Success(medicineDto);
        }
        catch (Exception ex)
        {
            return Result<MedicineDto>.Failure($"Error retrieving medicine: {ex.Message}");
        }
    }

    public async Task<Result<MedicineDto>> CreateAsync(CreateMedicineDto createMedicineDto)
    {
        try
        {
            var existingMedicine = await _unitOfWork.Repository<Medicine>()
                .GetByConditionAsync(m => m.Name.ToLower() == createMedicineDto.Name.ToLower());

            if (existingMedicine != null)
                return Result<MedicineDto>.Failure("Medicine with this name already exists");

            var medicine = _mapper.Map<Medicine>(createMedicineDto);

            await _unitOfWork.Repository<Medicine>().AddAsync(medicine);
            await _unitOfWork.SaveChangeAsync();

            var medicineDto = _mapper.Map<MedicineDto>(medicine);
            return Result<MedicineDto>.Success(medicineDto);
        }
        catch (Exception ex)
        {
            return Result<MedicineDto>.Failure($"Error creating medicine: {ex.Message}");
        }
    }

    public async Task<Result<MedicineDto>> UpdateAsync(int id, UpdateMedicineDto updateMedicineDto)
    {
        try
        {
            var medicine = await _unitOfWork.Repository<Medicine>().GetByIdAsync(id);
            if (medicine == null)
                return Result<MedicineDto>.Failure("Medicine not found");

            // Check if new name already exists (excluding current medicine)
            if (!string.IsNullOrEmpty(updateMedicineDto.Name) &&
                updateMedicineDto.Name.ToLower() != medicine.Name.ToLower())
            {
                var existingMedicine = await _unitOfWork.Repository<Medicine>()
                    .GetByConditionAsync(m => m.Name.ToLower() == updateMedicineDto.Name.ToLower());

                if (existingMedicine != null)
                    return Result<MedicineDto>.Failure("Medicine with this name already exists");
            }

            _mapper.Map(updateMedicineDto, medicine);

            _unitOfWork.Repository<Medicine>().Update(medicine);
            await _unitOfWork.SaveChangeAsync();

            var medicineDto = _mapper.Map<MedicineDto>(medicine);
            return Result<MedicineDto>.Success(medicineDto);
        }
        catch (Exception ex)
        {
            return Result<MedicineDto>.Failure($"Error updating medicine: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var medicine = await _unitOfWork.Repository<Medicine>()
                .GetAllQueryable(new[] { "PrescriptionDetails" })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medicine == null)
                return Result.Failure("Medicine not found");

            // Check if medicine is used in prescriptions
            if (medicine.PrescriptionDetails.Any())
                return Result.Failure("Cannot delete medicine that is used in prescriptions");

            _unitOfWork.Repository<Medicine>().Remove(medicine);
            await _unitOfWork.SaveChangeAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting medicine: {ex.Message}");
        }
    }
}
