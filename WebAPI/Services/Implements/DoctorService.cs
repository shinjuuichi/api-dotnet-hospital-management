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
    private readonly IGenericRepository<Doctor> _doctorRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Specialty> _specialtyRepository;
    private readonly IGenericRepository<Appointment> _appointmentRepository;

    public DoctorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _doctorRepository = _unitOfWork.Repository<Doctor>();
        _userRepository = _unitOfWork.Repository<User>();
        _specialtyRepository = _unitOfWork.Repository<Specialty>();
        _appointmentRepository = _unitOfWork.Repository<Appointment>();
    }

    public async Task<List<DoctorListResponseDto>> GetAllDoctorsAsync()
    {
        string[] includes = { "User", "Specialty" };
        var query = _doctorRepository
            .GetAllQueryable(includes);
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
            })
            .ToListAsync();

        return doctors;
    }

    public async Task<List<DoctorListResponseDto>> GetAllDoctorsIncludeDeletedAsync()
    {
        string[] includes = { "User", "Specialty" };
        var query = _doctorRepository
            .GetAllIncludeDeletedQueryable(includes);
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
        string[] includes = { "User", "Specialty" };
        var doctor = await _doctorRepository
            .GetAllQueryable(includes)
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
        var existingUser = await _userRepository
            .GetByConditionAsync(u => u.Email == request.Email);

        if (existingUser != null)
            throw new InvalidOperationException("Email already registered");

        var existingDoctor = await _doctorRepository
            .GetByConditionAsync(d => d.LicenseNo == request.LicenseNo);

        if (existingDoctor != null)
            throw new InvalidOperationException("License number already exists");

        var specialty = await _specialtyRepository.GetByIdAsync(request.SpecialtyId);
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
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangeAsync();

        var doctor = new Doctor
        {
            UserId = user.Id,
            LicenseNo = request.LicenseNo,
            Bio = request.Bio,
            YearOfExperience = request.YearOfExperience,
            SpecialtyId = request.SpecialtyId
        };

        await _doctorRepository.AddAsync(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(doctor.Id);
    }

    public async Task<DoctorResponseDto> UpdateDoctorAsync(int id, UpdateDoctorRequestDto request)
    {
        var doctor = await _doctorRepository
            .GetByIdAsync(id, new[] { "User" });

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        var existingDoctor = await _doctorRepository
            .GetByConditionAsync(d => d.LicenseNo == request.LicenseNo && d.Id != id);

        if (existingDoctor != null)
            throw new InvalidOperationException("License number already exists");

        var specialty = await _specialtyRepository.GetByIdAsync(request.SpecialtyId);
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

        _userRepository.Update(doctor.User);
        _doctorRepository.Update(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(id);
    }

    public async Task<DoctorResponseDto> ActivateDoctorAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        if (!doctor.IsDeleted)
            throw new InvalidOperationException("Doctor is already active");

        doctor.IsDeleted = false;
        _doctorRepository.Update(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(id);
    }

    public async Task<DoctorResponseDto> DeactivateDoctorAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);

        if (doctor == null)
            throw new InvalidOperationException("Doctor not found");

        if (doctor.IsDeleted)
            throw new InvalidOperationException("Doctor is already inactive");

        var hasActiveAppointments = await _appointmentRepository
            .AnyAsync(a => a.DoctorId == id && a.Status == AppointmentStatusEnum.Pending);

        if (hasActiveAppointments)
            throw new InvalidOperationException("Cannot deactivate doctor with pending appointments");

        doctor.IsDeleted = true;
        _doctorRepository.Update(doctor);
        await _unitOfWork.SaveChangeAsync();

        return await GetDoctorByIdAsync(id);
    }
}