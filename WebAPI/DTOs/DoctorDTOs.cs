using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Enum;

namespace WebAPI.DTOs;

public class DoctorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public GenderEnum Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
}

public class CreateDoctorDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public GenderEnum Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Required]
    public int SpecialtyId { get; set; }
}

public class UpdateDoctorDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public GenderEnum Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Required]
    public int SpecialtyId { get; set; }
}