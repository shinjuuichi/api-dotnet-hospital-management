using WebAPI.Models.EntityAbstractions;
using WebAPI.Models.Enum;

namespace WebAPI.Models;

public class Doctor : AuditableEntity
{
    public string LicenseNumber { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public GenderEnum Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int UserId { get; set; }

    public int SpecialtyId { get; set; }

    public User User { get; set; } = null!;
    public Specialty Specialty { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
