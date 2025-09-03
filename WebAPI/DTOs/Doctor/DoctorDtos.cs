using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Doctor;

public class CreateDoctorRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    public int Gender { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LicenseNo { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 2)]
    public string Bio { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int YearOfExperience { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SpecialtyId { get; set; }
}

public class UpdateDoctorRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    public int Gender { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LicenseNo { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 2)]
    public string Bio { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int YearOfExperience { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SpecialtyId { get; set; }
}

public class DoctorResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string? Avatar { get; set; }
    public string LicenseNo { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int YearOfExperience { get; set; }
    public SpecialtyInfoDto Specialty { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTime CreationDate { get; set; }
}

public class DoctorListResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string LicenseNo { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int YearOfExperience { get; set; }
    public SpecialtyInfoDto Specialty { get; set; } = null!;
    public bool IsDeleted { get; set; }
}

public class SpecialtyInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
