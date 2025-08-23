using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

[Index(nameof(LicenseNo), IsUnique = true, Name = "IX_Doctors_LicenseNo_Unique")]
public class Doctor : AuditableEntity
{
    [Required(ErrorMessage = "License number is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "License number must be between 2 and 100 characters")]
    public string LicenseNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bio is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Bio must be between 2 and 100 characters")]
    public string? Bio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Year of Experience is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Year of Experience must be a positive number")]
    public int YearOfExperience { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Specialty ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Specialty ID must be a positive number")]
    public int SpecialtyId { get; set; }

    public User User { get; set; } = null!;
    public Specialty Specialty { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
