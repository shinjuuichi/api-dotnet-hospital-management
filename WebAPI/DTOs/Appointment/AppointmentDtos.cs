using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Appointment;

public class CreateAppointmentRequestDto
{
    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;

    public int? DoctorId { get; set; }
}

public class AssignDoctorRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int AppointmentId { get; set; }
}

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public PatientInfoDto Patient { get; set; } = null!;
    public DoctorInfoDto? Doctor { get; set; }
    public DateTime CreationDate { get; set; }
    public bool HasPrescription { get; set; }
}

public class AppointmentListResponseDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public PatientInfoDto Patient { get; set; } = null!;
    public DoctorInfoDto? Doctor { get; set; }
    public bool HasPrescription { get; set; }
}

public class PatientInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public class DoctorInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string LicenseNo { get; set; } = string.Empty;
    public SpecialtyInfoDto Specialty { get; set; } = null!;
}

public class SpecialtyInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
