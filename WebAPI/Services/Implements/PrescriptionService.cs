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

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PrescriptionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PrescriptionDto>> CreateAsync(int doctorId, CreatePrescriptionDto createPrescriptionDto)
    {
        try
        {
            // Verify doctor exists
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
            if (doctor == null)
                return Result<PrescriptionDto>.Failure("Doctor not found");

            // Verify appointment exists and belongs to the doctor
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetByConditionAsync(a => a.Id == createPrescriptionDto.AppointmentId && a.DoctorId == doctorId);

            if (appointment == null)
                return Result<PrescriptionDto>.Failure("Appointment not found or does not belong to this doctor");

            // Verify appointment is completed
            if (appointment.Status != AppointmentStatusEnum.Completed)
                return Result<PrescriptionDto>.Failure("Prescription can only be created for completed appointments");

            // Check if prescription already exists for this appointment
            var existingPrescription = await _unitOfWork.Repository<Prescription>()
                .GetByConditionAsync(p => p.AppointmentId == createPrescriptionDto.AppointmentId);

            if (existingPrescription != null)
                return Result<PrescriptionDto>.Failure("Prescription already exists for this appointment");

            // Verify all medicines exist
            var medicineIds = createPrescriptionDto.PrescriptionDetails.Select(m => m.MedicineId).ToList();
            var medicines = await _unitOfWork.Repository<Medicine>()
                .GetAllAsync(m => medicineIds.Contains(m.Id));

            if (medicines.Count != medicineIds.Count)
                return Result<PrescriptionDto>.Failure("One or more medicines not found");

            // Create prescription
            var prescription = new Prescription
            {
                AppointmentId = createPrescriptionDto.AppointmentId,
                DoctorId = doctorId,
                Notes = createPrescriptionDto.Notes
            };

            await _unitOfWork.Repository<Prescription>().AddAsync(prescription);
            await _unitOfWork.SaveChangeAsync();

            // Create prescription details
            foreach (var medicineDto in createPrescriptionDto.PrescriptionDetails)
            {
                var prescriptionDetail = new PrescriptionDetail
                {
                    PrescriptionId = prescription.Id,
                    MedicineId = medicineDto.MedicineId,
                    Dosage = medicineDto.Dosage,
                    Frequency = medicineDto.Frequency,
                    Duration = medicineDto.Duration,
                    Instructions = medicineDto.Instructions
                };

                await _unitOfWork.Repository<PrescriptionDetail>().AddAsync(prescriptionDetail);
            }

            await _unitOfWork.SaveChangeAsync();

            // Reload prescription with includes
            prescription = await _unitOfWork.Repository<Prescription>()
                .GetAllQueryable(new[] { "Appointment", "Doctor", "Doctor.User", "Patient", "Patient.User", "PrescriptionDetails", "PrescriptionDetails.Medicine" })
                .FirstOrDefaultAsync(p => p.Id == prescription.Id);

            var prescriptionDto = _mapper.Map<PrescriptionDto>(prescription);
            return Result<PrescriptionDto>.Success(prescriptionDto);
        }
        catch (Exception ex)
        {
            return Result<PrescriptionDto>.Failure($"Error creating prescription: {ex.Message}");
        }
    }

    public async Task<Result<PrescriptionDto>> GetByAppointmentAsync(int appointmentId)
    {
        try
        {
            var prescription = await _unitOfWork.Repository<Prescription>()
                .GetAllQueryable(new[] { "Appointment", "Doctor", "Doctor.User", "Patient", "Patient.User", "PrescriptionDetails", "PrescriptionDetails.Medicine" })
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

            if (prescription == null)
                return Result<PrescriptionDto>.Failure("Prescription not found for this appointment");

            var prescriptionDto = _mapper.Map<PrescriptionDto>(prescription);
            return Result<PrescriptionDto>.Success(prescriptionDto);
        }
        catch (Exception ex)
        {
            return Result<PrescriptionDto>.Failure($"Error retrieving prescription: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<PrescriptionDto>>> GetByPatientAsync(int patientId, QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Prescription>()
                .GetAllQueryable(new[] { "Appointment", "Appointment.Patient", "Appointment.Patient.User", "Doctor", "Doctor.User", "PrescriptionDetails", "PrescriptionDetails.Medicine" })
                .Where(p => p.Appointment.PatientId == patientId);

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(p => 
                    p.Doctor.User.FullName.Contains(options.Search) ||
                    (p.Notes != null && p.Notes.Contains(options.Search)) ||
                    p.PrescriptionDetails.Any(pd => pd.Medicine.Name.Contains(options.Search)));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "doctor" => isDescending ? query.OrderByDescending(p => p.Doctor.User.FullName) : query.OrderBy(p => p.Doctor.User.FullName),
                "createdat" => isDescending ? query.OrderByDescending(p => p.CreationDate) : query.OrderBy(p => p.CreationDate),
                _ => query.OrderByDescending(p => p.CreationDate)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<PrescriptionDto>);
            return Result<PagedResult<PrescriptionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<PrescriptionDto>>.Failure($"Error retrieving patient prescriptions: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<PrescriptionDto>>> GetByDoctorAsync(int doctorId, QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Prescription>()
                .GetAllQueryable(new[] { "Appointment", "Appointment.Patient", "Appointment.Patient.User", "Doctor", "Doctor.User", "PrescriptionDetails", "PrescriptionDetails.Medicine" })
                .Where(p => p.DoctorId == doctorId);

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(p => 
                    p.Appointment.Patient.User.FullName.Contains(options.Search) ||
                    (p.Notes != null && p.Notes.Contains(options.Search)) ||
                    p.PrescriptionDetails.Any(pd => pd.Medicine.Name.Contains(options.Search)));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "patient" => isDescending ? query.OrderByDescending(p => p.Appointment.Patient.User.FullName) : query.OrderBy(p => p.Appointment.Patient.User.FullName),
                "date" => isDescending ? query.OrderByDescending(p => p.PrescriptionDate) : query.OrderBy(p => p.PrescriptionDate),
                "createdat" => isDescending ? query.OrderByDescending(p => p.CreationDate) : query.OrderBy(p => p.CreationDate),
                _ => query.OrderByDescending(p => p.CreationDate)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<PrescriptionDto>);
            return Result<PagedResult<PrescriptionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<PrescriptionDto>>.Failure($"Error retrieving doctor prescriptions: {ex.Message}");
        }
    }
}
