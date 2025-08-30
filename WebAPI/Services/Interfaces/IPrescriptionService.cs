using WebAPI.DTOs.Prescription;

namespace WebAPI.Services.Interfaces;

public interface IPrescriptionService
{
    Task<List<PrescriptionListResponseDto>> GetAllPrescriptionsAsync(int? userId = null, int? role = null);
    Task<PrescriptionResponseDto> GetPrescriptionByIdAsync(int id);
    Task<PrescriptionResponseDto> CreatePrescriptionAsync(int appointmentId, CreatePrescriptionRequestDto request, int doctorId);
}