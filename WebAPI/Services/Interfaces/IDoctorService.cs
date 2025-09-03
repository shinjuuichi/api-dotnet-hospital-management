using WebAPI.DTOs.Doctor;

namespace WebAPI.Services.Interfaces;

public interface IDoctorService
{
    Task<List<DoctorListResponseDto>> GetAllDoctorsAsync();
    Task<List<DoctorListResponseDto>> GetAllDoctorsIncludeDeletedAsync();
    Task<DoctorResponseDto> GetDoctorByIdAsync(int id);
    Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorRequestDto request);
    Task<DoctorResponseDto> UpdateDoctorAsync(int id, UpdateDoctorRequestDto request);
    Task<DoctorResponseDto> ActivateDoctorAsync(int id);
    Task<DoctorResponseDto> DeactivateDoctorAsync(int id);
}