using WebAPI.DTOs.User;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Utils;

namespace WebAPI.Services.Implements;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Patient> _patientRepository;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.Repository<User>();
        _patientRepository = _unitOfWork.Repository<Patient>();
    }

    public async Task<ProfileResponseDto> GetProfileAsync(int userId)
    {
        string[] includes = { "Patient" };
        var user = await _userRepository.GetByIdAsync(userId, includes)
            ?? throw new InvalidOperationException("User not found");
        var response = new ProfileResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Gender = (int)user.Gender,
            Role = (int)user.Role,
            Avatar = user.Avatar
        };

        if (user.Role == RoleEnum.Customer && user.Patient != null)
        {
            response.Address = user.Patient.Address;
            response.InsuranceNo = user.Patient.InsuranceNo;
        }

        return response;
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
    {
        string[] includes = { "Patient" };
        var user = await _userRepository.GetByIdAsync(userId, includes)
            ?? throw new InvalidOperationException("User not found");
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = (GenderEnum)request.Gender;
        if (request.AvatarFile != null)
        {
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                ImageUtil.RemoveImage(user.Avatar);
            }

            user.Avatar = await ImageUtil.SaveImageAsync(request.AvatarFile);
        }

        _userRepository.Update(user);

        if (user.Role == RoleEnum.Customer)
        {
            if (user.Patient == null)
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    Address = request.Address ?? string.Empty,
                    InsuranceNo = request.InsuranceNo
                };
                await _patientRepository.AddAsync(patient);
            }
            else
            {
                user.Patient.Address = request.Address ?? user.Patient.Address;
                user.Patient.InsuranceNo = request.InsuranceNo ?? user.Patient.InsuranceNo;
                _patientRepository.Update(user.Patient);
            }
        }

        await _unitOfWork.SaveChangeAsync();

        return await GetProfileAsync(userId);
    }
}