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

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DoctorService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<DoctorDto>>> GetAllAsync(QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Doctor>().GetAllQueryable(new[] { "User", "Specialty" });

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(d => 
                    d.User.FullName.Contains(options.Search) ||
                    d.Specialty.Name.Contains(options.Search) ||
                    d.LicenseNumber.Contains(options.Search));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(d => d.User.FullName) : query.OrderBy(d => d.User.FullName),
                "specialty" => isDescending ? query.OrderByDescending(d => d.Specialty.Name) : query.OrderBy(d => d.Specialty.Name),
                "license" => isDescending ? query.OrderByDescending(d => d.LicenseNumber) : query.OrderBy(d => d.LicenseNumber),
                "createdat" => isDescending ? query.OrderByDescending(d => d.CreationDate) : query.OrderBy(d => d.CreationDate),
                _ => query.OrderBy(d => d.User.FullName)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<DoctorDto>);
            return Result<PagedResult<DoctorDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<DoctorDto>>.Failure($"Error retrieving doctors: {ex.Message}");
        }
    }

    public async Task<Result<DoctorDto>> GetByIdAsync(int id)
    {
        try
        {
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetAllQueryable(new[] { "User", "Specialty", "Appointments" })
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return Result<DoctorDto>.Failure("Doctor not found");

            var doctorDto = _mapper.Map<DoctorDto>(doctor);
            return Result<DoctorDto>.Success(doctorDto);
        }
        catch (Exception ex)
        {
            return Result<DoctorDto>.Failure($"Error retrieving doctor: {ex.Message}");
        }
    }

    public async Task<Result<DoctorDto>> GetByUserIdAsync(int userId)
    {
        try
        {
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetAllQueryable(new[] { "User", "Specialty", "Appointments" })
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return Result<DoctorDto>.Failure("Doctor not found");

            var doctorDto = _mapper.Map<DoctorDto>(doctor);
            return Result<DoctorDto>.Success(doctorDto);
        }
        catch (Exception ex)
        {
            return Result<DoctorDto>.Failure($"Error retrieving doctor: {ex.Message}");
        }
    }

    public async Task<Result<DoctorDto>> CreateAsync(CreateDoctorDto createDoctorDto)
    {
        try
        {
            // First create the user
            var user = new User
            {
                FullName = createDoctorDto.FullName,
                Email = createDoctorDto.Email,
                Password = createDoctorDto.Password, // This should be hashed in real implementation
                Role = RoleEnum.Doctor
            };

            // Check if email already exists
            var existingUser = await _unitOfWork.Repository<User>()
                .GetByConditionAsync(u => u.Email == createDoctorDto.Email);

            if (existingUser != null)
                return Result<DoctorDto>.Failure("User with this email already exists");

            // Check if specialty exists
            var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(createDoctorDto.SpecialtyId);
            if (specialty == null)
                return Result<DoctorDto>.Failure("Specialty not found");

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangeAsync();

            var doctor = new Doctor
            {
                UserId = user.Id,
                SpecialtyId = createDoctorDto.SpecialtyId,
                LicenseNumber = createDoctorDto.LicenseNumber,
                PhoneNumber = createDoctorDto.PhoneNumber,
                Gender = createDoctorDto.Gender,
                DateOfBirth = createDoctorDto.DateOfBirth
            };

            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
            await _unitOfWork.SaveChangeAsync();

            // Reload doctor with related data
            doctor = await _unitOfWork.Repository<Doctor>()
                .GetAllQueryable(new[] { "User", "Specialty" })
                .FirstOrDefaultAsync(d => d.Id == doctor.Id);

            var doctorDto = _mapper.Map<DoctorDto>(doctor);
            return Result<DoctorDto>.Success(doctorDto);
        }
        catch (Exception ex)
        {
            return Result<DoctorDto>.Failure($"Error creating doctor: {ex.Message}");
        }
    }

    public async Task<Result<DoctorDto>> UpdateAsync(int id, UpdateDoctorDto updateDoctorDto)
    {
        try
        {
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetAllQueryable(new[] { "User", "Specialty" })
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return Result<DoctorDto>.Failure("Doctor not found");

            // Check if specialty exists
            var specialty = await _unitOfWork.Repository<Specialty>().GetByIdAsync(updateDoctorDto.SpecialtyId);
            if (specialty == null)
                return Result<DoctorDto>.Failure("Specialty not found");

            // Update doctor properties
            doctor.LicenseNumber = updateDoctorDto.LicenseNumber;
            doctor.PhoneNumber = updateDoctorDto.PhoneNumber;
            doctor.Gender = updateDoctorDto.Gender;
            doctor.DateOfBirth = updateDoctorDto.DateOfBirth;
            doctor.SpecialtyId = updateDoctorDto.SpecialtyId;

            // Update user properties
            doctor.User.FullName = updateDoctorDto.FullName;
            doctor.User.Email = updateDoctorDto.Email;

            _unitOfWork.Repository<Doctor>().Update(doctor);
            _unitOfWork.Repository<User>().Update(doctor.User);
            await _unitOfWork.SaveChangeAsync();

            var doctorDto = _mapper.Map<DoctorDto>(doctor);
            return Result<DoctorDto>.Success(doctorDto);
        }
        catch (Exception ex)
        {
            return Result<DoctorDto>.Failure($"Error updating doctor: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetAllQueryable(new[] { "Appointments" })
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return Result.Failure("Doctor not found");

            // Check if doctor has appointments
            if (doctor.Appointments.Any())
                return Result.Failure("Cannot delete doctor who has appointments");

            _unitOfWork.Repository<Doctor>().Remove(doctor);
            await _unitOfWork.SaveChangeAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting doctor: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<DoctorDto>>> GetBySpecialtyAsync(int specialtyId, QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<Doctor>()
                .GetAllQueryable(new[] { "User", "Specialty" })
                .Where(d => d.SpecialtyId == specialtyId);

            // Apply search
            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(d => 
                    d.User.FullName.Contains(options.Search) ||
                    d.LicenseNumber.Contains(options.Search));
            }

            // Apply sorting
            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(d => d.User.FullName) : query.OrderBy(d => d.User.FullName),
                "license" => isDescending ? query.OrderByDescending(d => d.LicenseNumber) : query.OrderBy(d => d.LicenseNumber),
                "createdat" => isDescending ? query.OrderByDescending(d => d.CreationDate) : query.OrderBy(d => d.CreationDate),
                _ => query.OrderBy(d => d.User.FullName)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<DoctorDto>);
            return Result<PagedResult<DoctorDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<DoctorDto>>.Failure($"Error retrieving doctors by specialty: {ex.Message}");
        }
    }
}
