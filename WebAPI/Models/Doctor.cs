using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Doctor : BaseEntity
{
    public int UserId { get; set; }

    public int SpecialtyId { get; set; }

    public User User { get; set; } = null!;
    public Specialty Specialty { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
