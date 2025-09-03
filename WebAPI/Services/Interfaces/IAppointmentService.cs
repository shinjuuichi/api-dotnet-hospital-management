using WebAPI.DTOs.Appointment;

namespace WebAPI.Services.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentListResponseDto>> GetAllAppointmentsAsync(int? userId = null, int? role = null);
    Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id);
    Task<AppointmentResponseDto> CreateAppointmentAsync(int userId, CreateAppointmentRequestDto request);
    Task DeleteAppointmentAsync(int id, int userId, int userRole);
    Task<AppointmentResponseDto> ConfirmAppointmentAsync(int id, int userId);
    Task<AppointmentResponseDto> CancelAppointmentAsync(int id, int userId, int userRole);
    Task<AppointmentResponseDto> CompleteAppointmentAsync(int id, int userId);
    Task<AppointmentResponseDto> AssignDoctorAsync(int id, AssignDoctorRequestDto request, int userId);
}