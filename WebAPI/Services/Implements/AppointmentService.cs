using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Appointment;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Services.StateMachine;

namespace WebAPI.Services.Implements;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Appointment> _appointmentRepository;
    private readonly IGenericRepository<Doctor> _doctorRepository;
    private readonly IGenericRepository<Patient> _patientRepository;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _appointmentRepository = _unitOfWork.Repository<Appointment>();
        _doctorRepository = _unitOfWork.Repository<Doctor>();
        _patientRepository = _unitOfWork.Repository<Patient>();
    }

    public async Task<List<AppointmentListResponseDto>> GetAllAppointmentsAsync(int? userId = null, int? role = null)
    {
        string[] includes = { "Patient.User", "Doctor.User", "Doctor.Specialty", "Prescription" };
        var query = _appointmentRepository
            .GetAllQueryable(includes);

        if (userId.HasValue && role.HasValue)
        {
            if (role == (int)RoleEnum.Customer)
            {
                query = query.Where(a => a.Patient.UserId == userId.Value);
            }
            else if (role == (int)RoleEnum.Doctor)
            {
                var doctor = await _doctorRepository
                    .GetByConditionAsync(d => d.UserId == userId.Value);
                if (doctor != null)
                {
                    query = query.Where(a => a.DoctorId == doctor.Id);
                }
                else
                {
                    throw new UnauthorizedAccessException("Doctor profile not found");
                }
            }
        }

        var appointments = await query
            .Select(a => new AppointmentListResponseDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Status = (int)a.Status,
                StatusName = a.Status.ToString(),
                Patient = new PatientInfoDto
                {
                    Id = a.Patient.Id,
                    FullName = a.Patient.User.FullName,
                    Email = a.Patient.User.Email,
                    PhoneNumber = a.Patient.User.PhoneNumber,
                    Avatar = a.Patient.User.Avatar
                },
                Doctor = a.Doctor != null ? new DoctorInfoDto
                {
                    Id = a.Doctor.Id,
                    FullName = a.Doctor.User.FullName,
                    Email = a.Doctor.User.Email,
                    Avatar = a.Doctor.User.Avatar,
                    LicenseNo = a.Doctor.LicenseNo,
                    Specialty = new SpecialtyInfoDto
                    {
                        Id = a.Doctor.Specialty.Id,
                        Name = a.Doctor.Specialty.Name
                    }
                } : null,
                HasPrescription = a.Prescription != null
            })
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return appointments;
    }

    public async Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id, int? userId = null, int? role = null)
    {
        string[] includes = { "Patient.User", "Doctor.User", "Doctor.Specialty", "Prescription" };
        var query = _appointmentRepository.GetAllQueryable(includes)
            .Where(a => a.Id == id);

        if (userId.HasValue && role.HasValue)
        {
            if (role == (int)RoleEnum.Customer)
            {
                query = query.Where(a => a.Patient.UserId == userId.Value);
            }
            else if (role == (int)RoleEnum.Doctor)
            {
                var doctor = await _doctorRepository
                    .GetByConditionAsync(d => d.UserId == userId.Value);
                if (doctor != null)
                {
                    query = query.Where(a => a.DoctorId == doctor.Id);
                }
                else
                {
                    throw new UnauthorizedAccessException("Doctor profile not found");
                }
            }
        }

        var appointment = await query
            .Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Status = (int)a.Status,
                StatusName = a.Status.ToString(),
                Patient = new PatientInfoDto
                {
                    Id = a.Patient.Id,
                    FullName = a.Patient.User.FullName,
                    Email = a.Patient.User.Email,
                    PhoneNumber = a.Patient.User.PhoneNumber,
                    Avatar = a.Patient.User.Avatar
                },
                Doctor = a.Doctor != null ? new DoctorInfoDto
                {
                    Id = a.Doctor.Id,
                    FullName = a.Doctor.User.FullName,
                    Email = a.Doctor.User.Email,
                    Avatar = a.Doctor.User.Avatar,
                    LicenseNo = a.Doctor.LicenseNo,
                    Specialty = new SpecialtyInfoDto
                    {
                        Id = a.Doctor.Specialty.Id,
                        Name = a.Doctor.Specialty.Name
                    }
                } : null,
                CreationDate = a.CreationDate ?? DateTime.UtcNow,
                HasPrescription = a.Prescription != null
            })
            .FirstOrDefaultAsync();

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        return appointment;
    }

    public async Task<AppointmentResponseDto> CreateAppointmentAsync(int userId, CreateAppointmentRequestDto request)
    {
        var patient = await _patientRepository
             .GetByConditionAsync(p => p.UserId == userId);

        if (patient == null)
            throw new InvalidOperationException("Patient profile not found");

        if (request.DoctorId.HasValue)
        {
            var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId.Value);
            if (doctor == null || doctor.IsDeleted)
                throw new InvalidOperationException("Doctor not found");
        }

        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Appointment date must be in the future");

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = request.DoctorId,
            AppointmentDate = request.AppointmentDate,
            Reason = request.Reason,
            Status = AppointmentStatusEnum.Pending
        };

        await _appointmentRepository.AddAsync(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(appointment.Id);
    }

    public async Task<AppointmentResponseDto> ConfirmAppointmentAsync(int id, int userId)
    {
        string[] includes = { "Doctor" };
        var appointment = await _appointmentRepository
            .GetByIdAsync(id, includes);

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        var doctor = await _doctorRepository
            .GetByConditionAsync(d => d.UserId == userId);

        if (doctor != null && appointment.DoctorId != doctor.Id)
            throw new UnauthorizedAccessException("You can only confirm appointments assigned to you");

        var stateMachine = new AppointmentStateMachine(appointment.Status);
        appointment.Status = stateMachine.ChangeState(AppointmentTrigger.Confirm);
        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(id);
    }

    public async Task<AppointmentResponseDto> CancelAppointmentAsync(int id, int userId, int userRole)
    {
        string[] includes = { "Patient", "Doctor" };
        var appointment = await _appointmentRepository.GetByIdAsync(id, includes);

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        if (userRole == (int)RoleEnum.Customer)
        {
            if (appointment.Patient.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own appointments");
        }
        else if (userRole == (int)RoleEnum.Doctor)
        {
            var doctor = await _doctorRepository.GetByIdAsync(userId);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                throw new UnauthorizedAccessException("You can only cancel appointments assigned to you");
        }

        var stateMachine = new AppointmentStateMachine(appointment.Status);
        appointment.Status = stateMachine.ChangeState(AppointmentTrigger.Cancel);
        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(id);
    }

    public async Task<AppointmentResponseDto> CompleteAppointmentAsync(int id, int userId)
    {
        string[] includes = { "Doctor" };
        var appointment = await _appointmentRepository.GetByIdAsync(id, includes);

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        var doctor = await _doctorRepository.GetByConditionAsync(d => d.UserId == userId);

        if (doctor == null || appointment.DoctorId != doctor.Id)
            throw new UnauthorizedAccessException("You can only complete appointments assigned to you");

        var stateMachine = new AppointmentStateMachine(appointment.Status);
        appointment.Status = stateMachine.ChangeState(AppointmentTrigger.Complete);
        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(id);
    }

    public async Task<AppointmentResponseDto> AssignDoctorAsync(int appointmentId, AssignDoctorRequestDto request)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
        if (doctor == null || doctor.IsDeleted)
            throw new InvalidOperationException("Doctor not found or inactive");

        if (appointment.Status != AppointmentStatusEnum.Pending)
            throw new InvalidOperationException("Can only assign doctor to pending appointments");

        appointment.DoctorId = request.DoctorId;
        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(appointmentId);
    }
}