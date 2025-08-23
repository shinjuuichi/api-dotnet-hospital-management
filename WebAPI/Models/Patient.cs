using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models.EntityAbstractions;
using WebAPI.Models.Enum;

namespace WebAPI.Models;

[Index(nameof(InsuranceNo), IsUnique = true, Name = "IX_Patients_InsuranceNo_Unique")]
public class Patient : AuditableEntity
{
    [Required(ErrorMessage = "Insurance number is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Insurance number must be between 2 and 100 characters")]
    public string InsuranceNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "User ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
    public int UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
