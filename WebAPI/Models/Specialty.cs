using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

[Index(nameof(Name), IsUnique = true, Name = "IX_Specialties_Name_Unique")]
public class Specialty : AuditableEntity
{
    [Required(ErrorMessage = "Specialty name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Specialty name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
