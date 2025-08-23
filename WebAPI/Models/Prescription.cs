using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Prescription : AuditableEntity
{
    public string? Notes { get; set; }

    public int AppointmentId { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
