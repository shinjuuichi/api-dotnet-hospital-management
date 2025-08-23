using System.ComponentModel.DataAnnotations;
using WebAPI.Models.EntityAbstractions;
using WebAPI.Models.Enum;

namespace WebAPI.Models;

public class Appointment : AuditableEntity
{
    [Required(ErrorMessage = "Appointment date is required")]
    [DataType(DataType.DateTime, ErrorMessage = "Please enter a valid date and time")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Reason for appointment is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 1000 characters")]
    public string Reason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Appointment status is required")]
    [EnumDataType(typeof(AppointmentStatusEnum), ErrorMessage = "Invalid appointment status")]
    public AppointmentStatusEnum Status { get; set; } = AppointmentStatusEnum.Pending;

    [Required(ErrorMessage = "Patient ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Patient ID must be a positive number")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Doctor ID must be a positive number")]
    public int? DoctorId { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor? Doctor { get; set; }
    public Prescription? Prescription { get; set; }
}
