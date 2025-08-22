using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Implements;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> RequestAsync(int patientId, RequestAppointmentDto requestDto)
    {
        try
        {
            // Verify patient exists
            var patient = await _unitOfWork.Repository<Patient>().GetByIdAsync(patientId);
            if (patient == null)
                return Result<AppointmentDto>.Failure("Patient not found");

            // Verify specialty exists
            var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(requestDto.SpecialtyId);
            if (specialty == null)
                return Result<AppointmentDto>.Failure("Specialty not found");

            var appointment = new Appointment
            {
                PatientId = patientId,
                SpecialtyId = requestDto.SpecialtyId,
                AppointmentDate = requestDto.AppointmentDate,
                Reason = requestDto.Reason,
                Status = AppointmentStatusEnum.Pending
            };

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await _unitOfWork.SaveChangeAsync();

            // Reload with includes
            appointment = await _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User" })
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Success(appointmentDto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Error requesting appointment: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<AppointmentDto>>> ListForCustomerAsync(int patientId, QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User" })
                .Where(a => a.PatientId == patientId);

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(a =>
                    a.Specialty.Name.Contains(options.Search) ||
                    (a.Doctor != null && a.Doctor.User.FullName.Contains(options.Search)) ||
                    a.Reason.Contains(options.Search));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "date" => isDescending ? query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate),
                "status" => isDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "specialty" => isDescending ? query.OrderByDescending(a => a.Specialty.Name) : query.OrderBy(a => a.Specialty.Name),
                "createdat" => isDescending ? query.OrderByDescending(a => a.CreationDate) : query.OrderBy(a => a.CreationDate),
                _ => query.OrderByDescending(a => a.CreationDate)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<AppointmentDto>);
            return Result<PagedResult<AppointmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<AppointmentDto>>.Failure($"Error retrieving customer appointments: {ex.Message}");
        }
    }

    public async Task<Result> CancelByCustomerAsync(int appointmentId, int patientId)
    {
        try
        {
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetByConditionAsync(a => a.Id == appointmentId && a.PatientId == patientId);

            if (appointment == null)
                return Result.Failure("Appointment not found");

            if (appointment.Status == AppointmentStatusEnum.Completed)
                return Result.Failure("Cannot cancel completed appointment");

            if (appointment.Status == AppointmentStatusEnum.Cancelled)
                return Result.Failure("Appointment is already cancelled");

            appointment.Status = AppointmentStatusEnum.Cancelled;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangeAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error cancelling appointment: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentDto>> AssignDoctorAsync(int appointmentId, AssignDoctorDto assignDto)
    {
        try
        {
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Specialty" })
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return Result<AppointmentDto>.Failure("Appointment not found");

            // Verify doctor exists and has the right specialty
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetAllQueryable(new[] { "User", "Specialty" })
                .FirstOrDefaultAsync(d => d.Id == assignDto.DoctorId);

            if (doctor == null)
                return Result<AppointmentDto>.Failure("Doctor not found");

            if (doctor.SpecialtyId != appointment.SpecialtyId)
                return Result<AppointmentDto>.Failure("Doctor specialty does not match appointment specialty");

            appointment.DoctorId = assignDto.DoctorId;
            appointment.Status = AppointmentStatusEnum.Confirmed;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangeAsync();

            // Reload with includes
            appointment = await _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User" })
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Success(appointmentDto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Error assigning doctor: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<AppointmentDto>>> GetAllAsync(QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User" });

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(a =>
                    a.Patient.User.FullName.Contains(options.Search) ||
                    a.Specialty.Name.Contains(options.Search) ||
                    (a.Doctor != null && a.Doctor.User.FullName.Contains(options.Search)));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "patient" => isDescending ? query.OrderByDescending(a => a.Patient.User.FullName) : query.OrderBy(a => a.Patient.User.FullName),
                "doctor" => isDescending ? query.OrderByDescending(a => a.Doctor!.User.FullName) : query.OrderBy(a => a.Doctor!.User.FullName),
                "specialty" => isDescending ? query.OrderByDescending(a => a.Specialty.Name) : query.OrderBy(a => a.Specialty.Name),
                "status" => isDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "date" => isDescending ? query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate),
                "createdat" => isDescending ? query.OrderByDescending(a => a.CreationDate) : query.OrderBy(a => a.CreationDate),
                _ => query.OrderByDescending(a => a.CreationDate)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<AppointmentDto>);
            return Result<PagedResult<AppointmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<AppointmentDto>>.Failure($"Error retrieving appointments: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentDto>> ConfirmAsync(int appointmentId, int doctorId)
    {
        try
        {
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetByConditionAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
                return Result<AppointmentDto>.Failure("Appointment not found");

            if (appointment.Status != AppointmentStatusEnum.Pending)
                return Result<AppointmentDto>.Failure("Only pending appointments can be confirmed");

            appointment.Status = AppointmentStatusEnum.Confirmed;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangeAsync();

            // Reload with includes
            appointment = await _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User" })
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Success(appointmentDto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Error confirming appointment: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentDto>> CompleteAsync(int appointmentId, int doctorId)
    {
        try
        {
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetByConditionAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
                return Result<AppointmentDto>.Failure("Appointment not found");

            if (appointment.Status != AppointmentStatusEnum.Confirmed)
                return Result<AppointmentDto>.Failure("Only confirmed appointments can be completed");

            appointment.Status = AppointmentStatusEnum.Completed;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangeAsync();

            // Reload with includes
            appointment = await _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User" })
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Success(appointmentDto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure($"Error completing appointment: {ex.Message}");
        }
    }

    public async Task<Result> CancelByDoctorAsync(int appointmentId, int doctorId)
    {
        try
        {
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetByConditionAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
                return Result.Failure("Appointment not found");

            if (appointment.Status == AppointmentStatusEnum.Completed)
                return Result.Failure("Cannot cancel completed appointment");

            if (appointment.Status == AppointmentStatusEnum.Cancelled)
                return Result.Failure("Appointment is already cancelled");

            appointment.Status = AppointmentStatusEnum.Cancelled;

            _unitOfWork.Repository<Appointment>().Update(appointment);
            await _unitOfWork.SaveChangeAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error cancelling appointment: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<AppointmentDto>>> GetDoctorAppointmentsAsync(int doctorId, QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User" })
                .Where(a => a.DoctorId == doctorId);

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(a =>
                    a.Patient.User.FullName.Contains(options.Search) ||
                    a.Reason.Contains(options.Search));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "patient" => isDescending ? query.OrderByDescending(a => a.Patient.User.FullName) : query.OrderBy(a => a.Patient.User.FullName),
                "status" => isDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "date" => isDescending ? query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate),
                "createdat" => isDescending ? query.OrderByDescending(a => a.CreationDate) : query.OrderBy(a => a.CreationDate),
                _ => query.OrderByDescending(a => a.AppointmentDate)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<AppointmentDto>);
            return Result<PagedResult<AppointmentDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<AppointmentDto>>.Failure($"Error retrieving doctor appointments: {ex.Message}");
        }
    }

    public async Task<Result<AppointmentDetailsDto>> GetByIdAsync(int appointmentId)
    {
        try
        {
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetAllQueryable(new[] { "Patient", "Patient.User", "Specialty", "Doctor", "Doctor.User", "Prescriptions" })
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return Result<AppointmentDetailsDto>.Failure("Appointment not found");

            var appointmentDto = _mapper.Map<AppointmentDetailsDto>(appointment);
            return Result<AppointmentDetailsDto>.Success(appointmentDto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDetailsDto>.Failure($"Error retrieving appointment: {ex.Message}");
        }
    }
}
