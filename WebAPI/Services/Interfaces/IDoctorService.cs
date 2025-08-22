using WebAPI.DTOs;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Interfaces;

public interface IDoctorService
{
    Task<Result<PagedResult<DoctorDto>>> GetAllAsync(QueryOptions options);
    Task<Result<DoctorDto>> GetByIdAsync(int id);
    Task<Result<DoctorDto>> GetByUserIdAsync(int userId);
    Task<Result<DoctorDto>> CreateAsync(CreateDoctorDto createDoctorDto);
    Task<Result<DoctorDto>> UpdateAsync(int id, UpdateDoctorDto updateDoctorDto);
    Task<Result> DeleteAsync(int id);
    Task<Result<PagedResult<DoctorDto>>> GetBySpecialtyAsync(int specialtyId, QueryOptions options);
}
