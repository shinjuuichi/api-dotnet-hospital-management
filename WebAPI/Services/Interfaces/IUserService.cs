using WebAPI.DTOs;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Interfaces;

public interface IUserService
{
    Task<Result<PagedResult<UserDto>>> GetAllAsync(QueryOptions options);
    Task<Result<UserDto>> GetByIdAsync(int id);
    Task<Result<UserDto>> CreateAsync(CreateUserDto createUserDto);
    Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto updateUserDto);
    Task<Result> DeleteAsync(int id);
    Task<Result<UserDto>> GetProfileAsync(int userId);
    Task<Result<UserDto>> UpdateProfileAsync(int userId, UpdateProfileDto updateProfileDto);
}
