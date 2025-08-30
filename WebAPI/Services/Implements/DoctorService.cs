using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Doctor;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Utils;

namespace WebAPI.Services.Implements;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;

    public DoctorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DoctorListResponseDto>> GetAllDoctorsAsync(bool includeInactive = false)
    {
        var query = _unitOfWork.Repository<Doctor>()
            .GetAllQueryable(new[] { "User", "Specialty" });

        if (!includeInactive)
            query = query.Where(d => !d.IsDeleted);

        var doctors = await query
            .Select(d => new DoctorListResponseDto
            {
                Id = d.Id,
                FullName = d.User.FullName,
                Email = d.User.Email,
                Avatar = d.User.Avatar,
                LicenseNo = d.LicenseNo,
                Bio = d.Bio!,
                YearOfExperience = d.YearOfExperience,
                Specialty = new SpecialtyInfoDto
                {
                    Id = d.Specialty.Id,
                    Name = d.Specialty.Name
                },
                IsDeleted = d.IsDeleted
            })
            .ToListAsync();

        return doctors;
    }

    public async Task<DoctorResponseDto> GetDoctorByIdAsync(int id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>()
            .GetAllQueryable(new[] { "User", "Specialty" })
            .Where(d => d.Id == id)
            .Select(d => new DoctorResponseDto
            {
                Id = d.Id,
                FullName = d.User.FullName,
                Email = d.User.Email,
                PhoneNumber = d.User.PhoneNumber,
                DateOfBirth = d.User.DateOfBirth,
                Gender = (int)d.User.Gender,
                Avatar = d.User.Avatar,
                LicenseNo = d.LicenseNo,
                Bio = d.Bio!,
                YearOfExperience = d.YearOfExperience,
                Specialty = new SpecialtyInfoDto
                {
                    Id = d.Specialty.Id,
                    Name = d.Specialty.Name
                },
                IsDeleted = d.IsDeleted,
                CreationDate = d.CreationDate ?? DateTime.UtcNow
            })
            .FirstOrDefaultAsync();

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        return doctor;
    }

    public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorRequestDto request)
    {
        var existingUser = await _unitOfWork.Repository<User>()
            .GetByConditionAsync(u => u.Email == request.Email);

        if (existingUser != null)
            throw new InvalidOperationException("Email already registered");

        var existingDoctor = await _unitOfWork.Repository<Doctor>()
            .GetByConditionAsync(d => d.LicenseNo == request.LicenseNo);

        if (existingDoctor != null)
            throw new InvalidOperationException("License number already exists");

        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId);
        if (specialty == null)
            throw new InvalidOperationException("Specialty not found");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = CryptoPassword.EncryptPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = (GenderEnum)request.Gender,
            Role = RoleEnum.Doctor,
            IsVerified = true
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangeAsync();

        var doctor = new Doctor
        {
            UserId = user.Id,
            LicenseNo = request.LicenseNo,
            Bio = request.Bio,
            YearOfExperience = request.YearOfExperience,
            SpecialtyId = request.SpecialtyId
        };

        await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(doctor.Id);
    }

    public async Task<DoctorResponseDto> UpdateDoctorAsync(int id, UpdateDoctorRequestDto request)
    {
        var doctor = await _unitOfWork.Repository<Doctor>()
            .GetByIdAsync(id, new[] { "User" });

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        var existingDoctor = await _unitOfWork.Repository<Doctor>()
            .GetByConditionAsync(d => d.LicenseNo == request.LicenseNo && d.Id != id);

        if (existingDoctor != null)
            throw new InvalidOperationException("License number already exists");

        var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(request.SpecialtyId);
        if (specialty == null)
            throw new InvalidOperationException("Specialty not found");

        doctor.User.FullName = request.FullName;
        doctor.User.PhoneNumber = request.PhoneNumber;
        doctor.User.DateOfBirth = request.DateOfBirth;
        doctor.User.Gender = (GenderEnum)request.Gender;

        doctor.LicenseNo = request.LicenseNo;
        doctor.Bio = request.Bio;
        doctor.YearOfExperience = request.YearOfExperience;
        doctor.SpecialtyId = request.SpecialtyId;

        _unitOfWork.Repository<User>().Update(doctor.User);
        _unitOfWork.Repository<Doctor>().Update(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(id);
    }

    public async Task DeleteDoctorAsync(int id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id);

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        var hasActiveAppointments = await _unitOfWork.Repository<Appointment>()
            .AnyAsync(a => a.DoctorId == id && a.Status != AppointmentStatusEnum.Cancelled && a.Status != AppointmentStatusEnum.Completed);

        if (hasActiveAppointments)
            throw new InvalidOperationException("Cannot delete doctor with active appointments");

        _unitOfWork.Repository<Doctor>().Remove(doctor);
        await _unitOfWork.SaveChangeAsync();
    }

    public async Task<DoctorResponseDto> ActivateDoctorAsync(int id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id);

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        if (!doctor.IsDeleted)
            throw new InvalidOperationException("Doctor is already active");

        doctor.IsDeleted = false;
        _unitOfWork.Repository<Doctor>().Update(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(id);
    }

    public async Task<DoctorResponseDto> DeactivateDoctorAsync(int id)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(id);

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        if (doctor.IsDeleted)
            throw new InvalidOperationException("Doctor is already inactive");

        var hasActiveAppointments = await _unitOfWork.Repository<Appointment>()
            .AnyAsync(a => a.DoctorId == id && a.Status == AppointmentStatusEnum.Pending);

        if (hasActiveAppointments)
            throw new InvalidOperationException("Cannot deactivate doctor with pending appointments");

        doctor.IsDeleted = true;
        _unitOfWork.Repository<Doctor>().Update(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(id);
    }
}