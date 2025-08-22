using WebAPI.Models.EntityAbstractions;
using WebAPI.Models.Enum;

namespace WebAPI.Models;

public class Appointment : AuditableEntity
{
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatusEnum Status { get; set; } = AppointmentStatusEnum.Pending;
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public int SpecialtyId { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor? Doctor { get; set; }
    public Specialty Specialty { get; set; } = null!;
    public Prescription? Prescription { get; set; }
}
