using WebAPI.DTOs.User;

namespace WebAPI.Services.Interfaces;

public interface IUserService
{
    Task<ProfileResponseDto> GetProfileAsync(int userId);
    Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request, IFormFile? avatarFile);
}