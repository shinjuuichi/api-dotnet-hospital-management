using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebAPI.DTOs.Auth;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Utils;

namespace WebAPI.Services.Implements;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Patient> _patientRepository;

    public AuthService(IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _userRepository = _unitOfWork.Repository<User>();
        _patientRepository = _unitOfWork.Repository<Patient>();
    }

    public async Task<string> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository
            .GetByConditionAsync(u => u.Email == request.Email);

        if (existingUser != null)
            throw new InvalidOperationException("Email already registered");

        var otp = OtpGenerator.GenerateOtp();

        var registrationData = new
        {
            request.FullName,
            request.Email,
            Password = CryptoPassword.EncryptPassword(request.Password),
            request.PhoneNumber,
            request.DateOfBirth,
            request.Gender,
            request.Address,
            request.InsuranceNo,
            Otp = otp
        };

        var cacheKey = $"registration:{request.Email}";
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(registrationData),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

        await EmailSender.SendEmailAsync(request.Email, "Registration OTP",
            $"Your registration OTP is: {otp}. This OTP will expire in 10 minutes.");

        return "OTP sent successfully. Please check your email.";
    }

    public async Task<AuthResponseDto> ConfirmRegistrationAsync(ConfirmRegistrationRequestDto request)
    {
        var cacheKey = $"registration:{request.Email}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(cachedData))
            throw new InvalidOperationException("Registration session expired. Please start registration again.");

        var registrationData = JsonSerializer.Deserialize<JsonElement>(cachedData);
        var storedOtp = registrationData.GetProperty("Otp").GetString();

        if (storedOtp != request.Otp)
            throw new InvalidOperationException("Invalid OTP");

        var user = new User
        {
            FullName = registrationData.GetProperty("FullName").GetString()!,
            Email = registrationData.GetProperty("Email").GetString()!,
            Password = registrationData.GetProperty("Password").GetString()!,
            PhoneNumber = registrationData.GetProperty("PhoneNumber").GetString()!,
            DateOfBirth = DateOnly.FromDateTime(registrationData.GetProperty("DateOfBirth").GetDateTime()),
            Gender = (GenderEnum)registrationData.GetProperty("Gender").GetInt32(),
            Role = RoleEnum.Customer,
            IsVerified = true
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangeAsync();

        var patient = new Patient
        {
            UserId = user.Id,
            Address = registrationData.GetProperty("Address").GetString()!,
            InsuranceNo = registrationData.GetProperty("InsuranceNo").GetString()
        };

        await _patientRepository.AddAsync(patient);
        await _unitOfWork.SaveChangeAsync();

        await _cache.RemoveAsync(cacheKey);

        var token = user.GenerateToken();

        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = (int)user.Gender,
                Role = (int)user.Role,
                Avatar = user.Avatar,
                IsVerified = user.IsVerified
            }
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetByConditionAsync(u => u.Email == request.Email);

        if (user == null || !CryptoPassword.IsPasswordCorrect(user.Password, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsVerified)
            throw new UnauthorizedAccessException("Account not verified");

        var token = user.GenerateToken();

        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = (int)user.Gender,
                Role = (int)user.Role,
                Avatar = user.Avatar,
                IsVerified = user.IsVerified
            }
        };
    }

    public async Task LogoutAsync(string token)
    {
        var cacheKey = $"blacklist:{token}";
        await _cache.SetStringAsync(cacheKey, "true", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetByConditionAsync(u => u.Email == request.Email);

        if (user == null)
            throw new InvalidOperationException("Email not found");

        var otp = OtpGenerator.GenerateOtp();
        var cacheKey = $"reset_password:{request.Email}";

        await _cache.SetStringAsync(cacheKey, otp, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        await EmailSender.SendEmailAsync(request.Email, "Password Reset OTP",
            $"Your password reset OTP is: {otp}. This OTP will expire in 10 minutes.");

        return "OTP sent successfully. Please check your email.";
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var cacheKey = $"reset_password:{request.Email}";
        var cachedOtp = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != request.Otp)
            throw new InvalidOperationException("Invalid or expired OTP");

        var user = await _unitOfWork.Repository<User>()
            .GetByConditionAsync(u => u.Email == request.Email);

        if (user == null)
            throw new InvalidOperationException("User not found");

        user.Password = CryptoPassword.EncryptPassword(request.NewPassword);
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangeAsync();

        await _cache.RemoveAsync(cacheKey);

        return "Password reset successfully";
    }
}