using WebAPI.DTOs;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Interfaces;

public interface IPrescriptionService
{
    Task<Result<PrescriptionDto>> CreateAsync(int doctorId, CreatePrescriptionDto createPrescriptionDto);
    Task<Result<PrescriptionDto>> GetByAppointmentAsync(int appointmentId);
    Task<Result<PagedResult<PrescriptionDto>>> GetByPatientAsync(int patientId, QueryOptions options);
    Task<Result<PagedResult<PrescriptionDto>>> GetByDoctorAsync(int doctorId, QueryOptions options);
}
