using WebAPI.DTOs.Specialty;

namespace WebAPI.Services.Interfaces;

public interface ISpecialtyService
{
    Task<List<SpecialtyResponseDto>> GetAllSpecialtiesAsync();
    Task<SpecialtyResponseDto> GetSpecialtyByIdAsync(int id);
    Task<SpecialtyResponseDto> CreateSpecialtyAsync(CreateSpecialtyRequestDto request);
    Task<SpecialtyResponseDto> UpdateSpecialtyAsync(int id, UpdateSpecialtyRequestDto request);
    Task DeleteSpecialtyAsync(int id);
}