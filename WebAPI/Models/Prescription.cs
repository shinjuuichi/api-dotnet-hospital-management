using System.ComponentModel.DataAnnotations;
using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Prescription : AuditableEntity
{
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Appointment ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Appointment ID must be a positive number")]
    public int AppointmentId { get; set; }

    public decimal TotalAmount { get; private set; }

    public Appointment Appointment { get; set; } = null!;
    public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
