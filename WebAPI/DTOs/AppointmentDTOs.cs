using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Enum;

namespace WebAPI.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatusEnum Status { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? SpecialtyName { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
}

public class RequestAppointmentDto
{
    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public int SpecialtyId { get; set; }
}

public class AssignDoctorDto
{
    [Required]
    public int DoctorId { get; set; }
}

public class AppointmentDetailsDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatusEnum Status { get; set; }
    public PatientDetailsDto Patient { get; set; } = null!;
    public DoctorDetailsDto? Doctor { get; set; }
    public SpecialtyDto Specialty { get; set; } = null!;
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
}

public class PatientDetailsDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
}

public class DoctorDetailsDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public SpecialtyDto Specialty { get; set; } = null!;
}