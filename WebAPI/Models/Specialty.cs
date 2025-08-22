using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Specialty : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
