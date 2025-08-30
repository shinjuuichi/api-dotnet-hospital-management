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

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProfileResponseDto> GetProfileAsync(int userId)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetByIdAsync(userId, new[] { "Patient" });

        if (user == null)
            throw new InvalidOperationException("User not found");

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

        // Include patient-specific data if user is a customer
        if (user.Role == RoleEnum.Customer && user.Patient != null)
        {
            response.Address = user.Patient.Address;
            response.InsuranceNo = user.Patient.InsuranceNo;
        }

        return response;
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request, IFormFile? avatarFile)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetByIdAsync(userId, new[] { "Patient" });

        if (user == null)
            throw new InvalidOperationException("User not found");

        // Update user information
        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = (GenderEnum)request.Gender;

        // Handle avatar upload
        if (avatarFile != null)
        {
            // Remove old avatar if exists
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                ImageUtil.RemoveImage(user.Avatar);
            }

            // Save new avatar
            user.Avatar = await ImageUtil.SaveImageAsync(avatarFile);
        }

        _unitOfWork.Repository<User>().Update(user);

        // Update patient information if user is a customer
        if (user.Role == RoleEnum.Customer)
        {
            if (user.Patient == null)
            {
                // Create patient record if it doesn't exist
                var patient = new Patient
                {
                    UserId = user.Id,
                    Address = request.Address ?? string.Empty,
                    InsuranceNo = request.InsuranceNo
                };
                await _unitOfWork.Repository<Patient>().AddAsync(patient);
            }
            else
            {
                // Update existing patient record
                user.Patient.Address = request.Address ?? user.Patient.Address;
                user.Patient.InsuranceNo = request.InsuranceNo ?? user.Patient.InsuranceNo;
                _unitOfWork.Repository<Patient>().Update(user.Patient);
            }
        }

        await _unitOfWork.SaveChangeAsync();

        return await GetProfileAsync(userId);
    }
}