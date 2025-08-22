using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebAPI.DTOs;
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
    private readonly IMapper _mapper;

    public AuthService(
        IUnitOfWork unitOfWork,
        IDistributedCache cache,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var existingUser = await _unitOfWork.Repository<User>()
                .GetByConditionAsync(u => u.Email == registerDto.Email);

            if (existingUser != null)
            {
                return Result<RegisterResponseDto>.Failure("Email already exists");
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Password = CryptoPassword.EncryptPassword(registerDto.Password),
                Role = RoleEnum.Customer,
                IsVerified = false // Store email verification status in database
            };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangeAsync();
            
            if (user.Role == RoleEnum.Customer)
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    PhoneNumber = "",
                    Gender = GenderEnum.Other
                };
                await _unitOfWork.Repository<Patient>().AddAsync(patient);
                await _unitOfWork.SaveChangeAsync();
            }

            var otp = OtpGenerator.GenerateOtp();

            var otpCacheKey = $"otp_{registerDto.Email}";
            var otpOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(otpCacheKey, otp, otpOptions);

            var throttleKey = $"otp_throttle_{registerDto.Email}";
            var throttleValue = await _cache.GetStringAsync(throttleKey);
            var sendCount = string.IsNullOrEmpty(throttleValue) ? 0 : int.Parse(throttleValue);
            
            if (sendCount >= 3)
            {
                return Result<RegisterResponseDto>.Failure("Too many OTP requests. Please try again later.");
            }

            var throttleOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };
            await _cache.SetStringAsync(throttleKey, (sendCount + 1).ToString(), throttleOptions);

            try
            {
                await EmailSender.SendEmailAsync(
                    registerDto.Email,
                    "Email Verification - Skibidi Hopita",
                    $"Your verification code is: {otp}. This code will expire in 10 minutes.");
            }
            catch (Exception ex)
            {
                return Result<RegisterResponseDto>.Failure($"Failed to send OTP email: {ex.Message}");
            }

            return Result<RegisterResponseDto>.Success(new RegisterResponseDto { Message = "OTP sent to your email" }, "OTP sent to your email");
        }
        catch (Exception ex)
        {
            return Result<RegisterResponseDto>.Failure($"Registration failed: {ex.Message}");
        }
    }

    public async Task<Result> ConfirmOtpAsync(ConfirmOtpDto confirmOtpDto)
    {
        var otpCacheKey = $"otp_{confirmOtpDto.Email}";
        var cachedOtp = await _cache.GetStringAsync(otpCacheKey);

        if (string.IsNullOrEmpty(cachedOtp))
        {
            return Result.Failure("OTP has expired or not found");
        }

        if (cachedOtp != confirmOtpDto.Otp)
        {
            return Result.Failure("Invalid OTP");
        }

        var user = await _unitOfWork.Repository<User>()
            .GetByConditionAsync(u => u.Email == confirmOtpDto.Email);

        if (user == null)
        {
            return Result.Failure("User not found");
        }

        user.IsVerified = true;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangeAsync();

        await _cache.RemoveAsync(otpCacheKey);

        return Result.Success("Email verified successfully");
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _unitOfWork.Repository<User>()
            .GetByConditionAsync(u => u.Email == loginDto.Email);

        if (user == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid email or password");
        }

        if (!user.IsVerified)
        {
            return Result<LoginResponseDto>.Failure("Email not verified. Please verify your email first.");
        }

        if (!CryptoPassword.IsPasswordCorrect(user.Password, loginDto.Password))
        {
            return Result<LoginResponseDto>.Failure("Invalid email or password");
        }

        var token = JwtHandler.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Role = user.Role.ToString(),
            UserId = user.Id,
            FullName = user.FullName
        }, "Login successful");
    }

    public async Task<Result<RegisterResponseDto>> ResendOtpAsync(ResendOtpDto resendOtpDto)
    {
        var throttleKey = $"otp_throttle_{resendOtpDto.Email}";
        var throttleValue = await _cache.GetStringAsync(throttleKey);
        var sendCount = string.IsNullOrEmpty(throttleValue) ? 0 : int.Parse(throttleValue);
        
        if (sendCount >= 3)
        {
            return Result<RegisterResponseDto>.Failure("Too many OTP requests. Please try again later.");
        }

        var user = await _unitOfWork.Repository<User>()
            .GetByConditionAsync(u => u.Email == resendOtpDto.Email);

        if (user == null)
        {
            return Result<RegisterResponseDto>.Failure("User not found");
        }

        var otp = OtpGenerator.GenerateOtp();

        var otpCacheKey = $"otp_{resendOtpDto.Email}";
        var otpOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        await _cache.SetStringAsync(otpCacheKey, otp, otpOptions);

        var throttleOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        await _cache.SetStringAsync(throttleKey, (sendCount + 1).ToString(), throttleOptions);

        try
        {
            await EmailSender.SendEmailAsync(
                resendOtpDto.Email,
                "Email Verification - Skibidi Hopita",
                $"Your verification code is: {otp}. This code will expire in 10 minutes.");
        }
        catch (Exception ex)
        {
            return Result<RegisterResponseDto>.Failure($"Failed to send OTP email: {ex.Message}");
        }

        return Result<RegisterResponseDto>.Success(new RegisterResponseDto { Message = "OTP resent to your email" }, "OTP resent to your email");
    }
}
