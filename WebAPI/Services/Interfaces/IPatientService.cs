using WebAPI.DTOs.Patient;

namespace WebAPI.Services.Interfaces;

public interface IPatientService
{
    Task<List<PatientListResponseDto>> GetAllPatientsAsync();
    Task<PatientResponseDto> GetPatientByIdAsync(int id);
}
