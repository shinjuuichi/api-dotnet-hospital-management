using WebAPI.DTOs;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Interfaces;

public interface IMedicineService
{
    Task<Result<PagedResult<MedicineDto>>> GetAllAsync(QueryOptions options);
    Task<Result<MedicineDto>> GetByIdAsync(int id);
    Task<Result<MedicineDto>> CreateAsync(CreateMedicineDto createMedicineDto);
    Task<Result<MedicineDto>> UpdateAsync(int id, UpdateMedicineDto updateMedicineDto);
    Task<Result> DeleteAsync(int id);
}
