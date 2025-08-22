using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Interfaces;
using WebAPI.Utils;
using WebAPI.Utils.Query;

namespace WebAPI.Services.Implements;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<UserDto>>> GetAllAsync(QueryOptions options)
    {
        try
        {
            var query = _unitOfWork.Repository<User>().GetAllQueryable();

            if (!string.IsNullOrEmpty(options.Search))
            {
                query = query.Where(u =>
                    u.FullName.Contains(options.Search) ||
                    u.Email.Contains(options.Search));
            }

            var isDescending = options.SortDir?.ToLower() == "desc";
            query = options.SortBy?.ToLower() switch
            {
                "fullname" => isDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "role" => isDescending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
                "createdat" => isDescending ? query.OrderByDescending(u => u.CreationDate) : query.OrderBy(u => u.CreationDate),
                _ => query.OrderBy(u => u.Id)
            };

            var result = await query.ToPagedResultAsync(options, _mapper.Map<UserDto>);
            return Result<PagedResult<UserDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<UserDto>>.Failure($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>()
                .GetAllQueryable(new[] { "Patient", "Doctor" })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return Result<UserDto>.Failure("User not found");

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto createUserDto)
    {
        try
        {
            var existingUser = await _unitOfWork.Repository<User>()
                .GetByConditionAsync(u => u.Email == createUserDto.Email);

            if (existingUser != null)
                return Result<UserDto>.Failure("User with this email already exists");

            var user = _mapper.Map<User>(createUserDto);
            user.Password = CryptoPassword.EncryptPassword(createUserDto.Password);

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangeAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Error creating user: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null)
                return Result<UserDto>.Failure("User not found");

            if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
            {
                var existingUser = await _unitOfWork.Repository<User>()
                    .GetByConditionAsync(u => u.Email == updateUserDto.Email);

                if (existingUser != null)
                    return Result<UserDto>.Failure("User with this email already exists");
            }

            _mapper.Map(updateUserDto, user);

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangeAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Error updating user: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null)
                return Result.Failure("User not found");

            _unitOfWork.Repository<User>().Remove(user);
            await _unitOfWork.SaveChangeAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> GetProfileAsync(int userId)
    {
        return await GetByIdAsync(userId);
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(int userId, UpdateProfileDto updateProfileDto)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
            if (user == null)
                return Result<UserDto>.Failure("User not found");

            if (!string.IsNullOrEmpty(updateProfileDto.Email) && updateProfileDto.Email != user.Email)
            {
                var existingUser = await _unitOfWork.Repository<User>()
                    .GetByConditionAsync(u => u.Email == updateProfileDto.Email);

                if (existingUser != null)
                    return Result<UserDto>.Failure("User with this email already exists");
            }

            _mapper.Map(updateProfileDto, user);

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangeAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Error updating profile: {ex.Message}");
        }
    }
}
