using WebAPI.DTOs;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Interfaces;

public interface IAppointmentService
{
    Task<Result<AppointmentDto>> RequestAsync(int patientId, RequestAppointmentDto requestDto);
    Task<Result<PagedResult<AppointmentDto>>> ListForCustomerAsync(int patientId, QueryOptions options);
    Task<Result> CancelByCustomerAsync(int appointmentId, int patientId);
    Task<Result<AppointmentDto>> AssignDoctorAsync(int appointmentId, AssignDoctorDto assignDto);
    Task<Result<PagedResult<AppointmentDto>>> GetAllAsync(QueryOptions options);
    Task<Result<AppointmentDto>> ConfirmAsync(int appointmentId, int doctorId);
    Task<Result<AppointmentDto>> CompleteAsync(int appointmentId, int doctorId);
    Task<Result> CancelByDoctorAsync(int appointmentId, int doctorId);
    Task<Result<PagedResult<AppointmentDto>>> GetDoctorAppointmentsAsync(int doctorId, QueryOptions options);
    Task<Result<AppointmentDetailsDto>> GetByIdAsync(int appointmentId);
}
