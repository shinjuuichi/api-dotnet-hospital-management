using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Prescription : AuditableEntity
{
    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
