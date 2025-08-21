using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Prescription : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int AppointmentId { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
