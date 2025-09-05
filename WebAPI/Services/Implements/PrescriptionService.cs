using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Prescription;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;

namespace WebAPI.Services.Implements;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Prescription> _prescriptionRepository;
    private readonly IGenericRepository<Doctor> _doctorRepository;
    private readonly IGenericRepository<Appointment> _appointmentRepository;
    private readonly IGenericRepository<Medicine> _medicineRepository;
    private readonly IGenericRepository<PrescriptionDetail> _prescriptionDetailRepository;

    public PrescriptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _prescriptionRepository = _unitOfWork.Repository<Prescription>();
        _doctorRepository = _unitOfWork.Repository<Doctor>();
        _appointmentRepository = _unitOfWork.Repository<Appointment>();
        _medicineRepository = _unitOfWork.Repository<Medicine>();
        _prescriptionDetailRepository = _unitOfWork.Repository<PrescriptionDetail>();
    }

    public async Task<List<PrescriptionListResponseDto>> GetAllPrescriptionsAsync(int? userId = null, int? role = null)
    {
        var query = _prescriptionRepository
            .GetAllQueryable(new[] { "Appointment.Patient.User", "Appointment.Doctor.User", "PrescriptionDetails" });

        // Filter based on user role
        if (userId.HasValue && role.HasValue)
        {
            if (role == (int)RoleEnum.Customer)
            {
                // Patient can only see their own prescriptions
                query = query.Where(p => p.Appointment.Patient.UserId == userId.Value);
            }
            else if (role == (int)RoleEnum.Doctor)
            {
                // Doctor can only see prescriptions they created
                var doctor = await _doctorRepository
                    .GetByConditionAsync(d => d.UserId == userId.Value);
                if (doctor != null)
                {
                    query = query.Where(p => p.Appointment.DoctorId == doctor.Id);
                }
            }
            // Managers can see all prescriptions (no additional filter)
        }

        var prescriptions = await query
            .Select(p => new PrescriptionListResponseDto
            {
                Id = p.Id,
                Notes = p.Notes,
                TotalAmount = p.TotalAmount,
                Appointment = new AppointmentInfoDto
                {
                    Id = p.Appointment.Id,
                    AppointmentDate = p.Appointment.AppointmentDate,
                    Reason = p.Appointment.Reason,
                    Patient = new PatientInfoDto
                    {
                        Id = p.Appointment.Patient.Id,
                        FullName = p.Appointment.Patient.User.FullName,
                        Email = p.Appointment.Patient.User.Email
                    },
                    Doctor = new DoctorInfoDto
                    {
                        Id = p.Appointment.Doctor!.Id,
                        FullName = p.Appointment.Doctor.User.FullName,
                        LicenseNo = p.Appointment.Doctor.LicenseNo
                    }
                },
                CreationDate = p.CreationDate ?? DateTime.UtcNow,
                DetailCount = p.PrescriptionDetails.Count
            })
            .OrderByDescending(p => p.CreationDate)
            .ToListAsync();

        return prescriptions;
    }

    public async Task<PrescriptionResponseDto> GetPrescriptionByIdAsync(int id)
    {
        var prescription = await _prescriptionRepository
            .GetAllQueryable(new[] { "Appointment.Patient.User", "Appointment.Doctor.User", "PrescriptionDetails.Medicine" })
            .Where(p => p.Id == id)
            .Select(p => new PrescriptionResponseDto
            {
                Id = p.Id,
                Notes = p.Notes,
                TotalAmount = p.TotalAmount,
                Appointment = new AppointmentInfoDto
                {
                    Id = p.Appointment.Id,
                    AppointmentDate = p.Appointment.AppointmentDate,
                    Reason = p.Appointment.Reason,
                    Patient = new PatientInfoDto
                    {
                        Id = p.Appointment.Patient.Id,
                        FullName = p.Appointment.Patient.User.FullName,
                        Email = p.Appointment.Patient.User.Email
                    },
                    Doctor = new DoctorInfoDto
                    {
                        Id = p.Appointment.Doctor!.Id,
                        FullName = p.Appointment.Doctor.User.FullName,
                        LicenseNo = p.Appointment.Doctor.LicenseNo
                    }
                },
                Details = p.PrescriptionDetails.Select(d => new PrescriptionDetailResponseDto
                {
                    Id = d.Id,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice,
                    UsageInstruction = d.UsageInstruction,
                    Medicine = new MedicineInfoDto
                    {
                        Id = d.Medicine.Id,
                        Name = d.Medicine.Name,
                        Price = d.Medicine.Price
                    }
                }).ToList(),
                CreationDate = p.CreationDate ?? DateTime.UtcNow
            })
            .FirstOrDefaultAsync();

        if (prescription == null)
            throw new InvalidOperationException("Prescription not found");

        return prescription;
    }

    public async Task<PrescriptionResponseDto> CreatePrescriptionAsync(int appointmentId, CreatePrescriptionRequestDto request, int userId)
    {
        var doctor = await _doctorRepository
            .GetByConditionAsync(d => d.UserId == userId);
        if (doctor == null)
            throw new InvalidOperationException("Doctor profile not found");

        string[] includes = { "Doctor", "Prescription" };
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, includes);
        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        if (appointment.DoctorId != doctor.Id)
            throw new UnauthorizedAccessException("You can only prescribe for your own appointments");

        if (appointment.Status != AppointmentStatusEnum.Completed)
            throw new InvalidOperationException("Can only prescribe for completed appointments");

        if (appointment.Prescription != null)
            throw new InvalidOperationException("Prescription already exists for this appointment");

        var medicineIds = request.Details.Select(d => d.MedicineId).ToList();
        var medicines = await _medicineRepository.GetAllAsync(m => medicineIds.Contains(m.Id));

        if (medicines.Count != medicineIds.Count)
            throw new InvalidOperationException("One or more medicines not found");

        var prescription = new Prescription
        {
            AppointmentId = appointmentId,
            Notes = request.Notes
        };

        await _prescriptionRepository.AddAsync(prescription);
        await _unitOfWork.SaveChangeAsync();

        decimal totalAmount = 0;
        foreach (var detailRequest in request.Details)
        {
            var medicine = medicines.First(m => m.Id == detailRequest.MedicineId);
            var detail = new PrescriptionDetail
            {
                PrescriptionId = prescription.Id,
                MedicineId = detailRequest.MedicineId,
                Quantity = detailRequest.Quantity,
                UnitPrice = medicine.Price,
                UsageInstruction = detailRequest.UsageInstruction
            };

            totalAmount += detail.TotalPrice;
            await _prescriptionDetailRepository.AddAsync(detail);
        }

        prescription.UpdateTotalAmount(totalAmount);

        _prescriptionRepository.Update(prescription);
        await _unitOfWork.SaveChangeAsync();

        return await GetPrescriptionByIdAsync(prescription.Id);
    }
}