using WebAPI.DTOs;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Interfaces;

public interface ISpecialtyService
{
    Task<Result<PagedResult<SpecialtyDto>>> GetAllAsync(QueryOptions options);
    Task<Result<SpecialtyDto>> GetByIdAsync(int id);
    Task<Result<SpecialtyDto>> CreateAsync(CreateSpecialtyDto createSpecialtyDto);
    Task<Result<SpecialtyDto>> UpdateAsync(int id, UpdateSpecialtyDto updateSpecialtyDto);
    Task<Result> DeleteAsync(int id);
}
