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

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AppointmentListResponseDto>> GetAllAppointmentsAsync(int? userId = null, int? role = null)
    {
        var query = _unitOfWork.Repository<Appointment>()
            .GetAllQueryable(new[] { "Patient.User", "Doctor.User", "Doctor.Specialty", "Prescription" });

        // Filter based on user role
        if (userId.HasValue && role.HasValue)
        {
            if (role == (int)RoleEnum.Customer)
            {
                // Patient can only see their own appointments
                query = query.Where(a => a.Patient.UserId == userId.Value);
            }
            else if (role == (int)RoleEnum.Doctor)
            {
                // Doctor can only see their assigned appointments
                var doctor = await _unitOfWork.Repository<Doctor>()
                    .GetByConditionAsync(d => d.UserId == userId.Value);
                if (doctor != null)
                {
                    query = query.Where(a => a.DoctorId == doctor.Id);
                }
            }
            // Managers can see all appointments (no additional filter)
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

    public async Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id)
    {
        var appointment = await _unitOfWork.Repository<Appointment>()
            .GetAllQueryable(new[] { "Patient.User", "Doctor.User", "Doctor.Specialty", "Prescription" })
            .Where(a => a.Id == id)
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

    public async Task<AppointmentResponseDto> CreateAppointmentAsync(int patientId, CreateAppointmentRequestDto request)
    {
        // Verify patient exists
        var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(patientId);
        if (patient == null)
            throw new InvalidOperationException("Patient not found");

        // Verify doctor exists if specified
        if (request.DoctorId.HasValue)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(request.DoctorId.Value);
            if (doctor == null || doctor.IsDeleted)
                throw new InvalidOperationException("Doctor not found or inactive");
        }

        // Check if appointment date is in the future
        if (request.AppointmentDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Appointment date must be in the future");

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = request.DoctorId,
            AppointmentDate = request.AppointmentDate,
            Reason = request.Reason,
            Status = AppointmentStatusEnum.Pending
        };

        await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(appointment.Id);
    }

    public async Task DeleteAppointmentAsync(int id, int userId, int userRole)
    {
        var appointment = await _unitOfWork.Repository<Appointment>()
            .GetByIdAsync(id, new[] { "Patient" });

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        // Check permissions
        if (userRole == (int)RoleEnum.Customer)
        {
            // Patients can only delete their own appointments
            if (appointment.Patient.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own appointments");
        }

        // Can only delete pending appointments
        if (appointment.Status != AppointmentStatusEnum.Pending)
            throw new InvalidOperationException("Can only delete pending appointments");

        _unitOfWork.Repository<Appointment>().Remove(appointment);
        await _unitOfWork.SaveChangeAsync();
    }

    public async Task<AppointmentResponseDto> ConfirmAppointmentAsync(int id, int userId)
    {
        var appointment = await _unitOfWork.Repository<Appointment>()
            .GetByIdAsync(id, new[] { "Doctor" });

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        // Only doctors can confirm appointments assigned to them, or managers can confirm any
        var doctor = await _unitOfWork.Repository<Doctor>()
            .GetByConditionAsync(d => d.UserId == userId);

        if (doctor != null && appointment.DoctorId != doctor.Id)
            throw new UnauthorizedAccessException("You can only confirm appointments assigned to you");

        // Use state machine to validate transition
        var stateMachine = new AppointmentStateMachine(appointment.Status);
        if (!stateMachine.CanFire(AppointmentTrigger.Confirm))
            throw new InvalidOperationException($"Cannot confirm appointment in {appointment.Status} status");

        appointment.Status = stateMachine.Fire(AppointmentTrigger.Confirm);
        _unitOfWork.Repository<Appointment>().Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(id);
    }

    public async Task<AppointmentResponseDto> CancelAppointmentAsync(int id, int userId, int userRole)
    {
        var appointment = await _unitOfWork.Repository<Appointment>()
            .GetByIdAsync(id, new[] { "Patient", "Doctor" });

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        // Check permissions
        if (userRole == (int)RoleEnum.Customer)
        {
            // Patients can only cancel their own appointments
            if (appointment.Patient.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own appointments");
        }
        else if (userRole == (int)RoleEnum.Doctor)
        {
            // Doctors can only cancel appointments assigned to them
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetByConditionAsync(d => d.UserId == userId);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                throw new UnauthorizedAccessException("You can only cancel appointments assigned to you");
        }

        // Use state machine to validate transition
        var stateMachine = new AppointmentStateMachine(appointment.Status);
        if (!stateMachine.CanFire(AppointmentTrigger.Cancel))
            throw new InvalidOperationException($"Cannot cancel appointment in {appointment.Status} status");

        appointment.Status = stateMachine.Fire(AppointmentTrigger.Cancel);
        _unitOfWork.Repository<Appointment>().Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(id);
    }

    public async Task<AppointmentResponseDto> CompleteAppointmentAsync(int id, int userId)
    {
        var appointment = await _unitOfWork.Repository<Appointment>()
            .GetByIdAsync(id, new[] { "Doctor" });

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        // Only doctors can complete appointments assigned to them
        var doctor = await _unitOfWork.Repository<Doctor>()
            .GetByConditionAsync(d => d.UserId == userId);

        if (doctor == null || appointment.DoctorId != doctor.Id)
            throw new UnauthorizedAccessException("You can only complete appointments assigned to you");

        // Use state machine to validate transition
        var stateMachine = new AppointmentStateMachine(appointment.Status);
        if (!stateMachine.CanFire(AppointmentTrigger.Complete))
            throw new InvalidOperationException($"Cannot complete appointment in {appointment.Status} status");

        appointment.Status = stateMachine.Fire(AppointmentTrigger.Complete);
        _unitOfWork.Repository<Appointment>().Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(id);
    }

    public async Task<AppointmentResponseDto> AssignDoctorAsync(int id, AssignDoctorRequestDto request, int userId)
    {
        var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(id);

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        // Verify doctor exists and is active
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(request.DoctorId);
        if (doctor == null || doctor.IsDeleted)
            throw new InvalidOperationException("Doctor not found or inactive");

        // Can only assign doctor to pending appointments
        if (appointment.Status != AppointmentStatusEnum.Pending)
            throw new InvalidOperationException("Can only assign doctor to pending appointments");

        appointment.DoctorId = request.DoctorId;
        _unitOfWork.Repository<Appointment>().Update(appointment);
        await _unitOfWork.SaveChangeAsync();

        return await GetAppointmentByIdAsync(id);
    }
}