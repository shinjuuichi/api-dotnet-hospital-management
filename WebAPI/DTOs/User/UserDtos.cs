using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.User;

public class UpdateProfileRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public int Gender { get; set; }

    public string? Address { get; set; }
    public string? InsuranceNo { get; set; }
}

public class ProfileResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Gender { get; set; }
    public int Role { get; set; }
    public string? Avatar { get; set; }
    public string? Address { get; set; }
    public string? InsuranceNo { get; set; }
}
