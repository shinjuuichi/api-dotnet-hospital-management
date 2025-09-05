using WebAPI.DTOs.Appointment;

namespace WebAPI.Services.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentListResponseDto>> GetAllAppointmentsAsync(int? userId = null, int? role = null);
    Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id, int? userId = null, int? role = null);
    Task<AppointmentResponseDto> CreateAppointmentAsync(int userId, CreateAppointmentRequestDto request);
    Task<AppointmentResponseDto> ConfirmAppointmentAsync(int id, int userId);
    Task<AppointmentResponseDto> CancelAppointmentAsync(int id, int userId, int userRole);
    Task<AppointmentResponseDto> CompleteAppointmentAsync(int id, int userId);
    Task<AppointmentResponseDto> AssignDoctorAsync(int appointmentId, AssignDoctorRequestDto request);
}