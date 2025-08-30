using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Prescription;

public class CreatePrescriptionRequestDto
{
    public string? Notes { get; set; }

    [Required]
    public List<PrescriptionDetailRequestDto> Details { get; set; } = new();
}

public class PrescriptionDetailRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int MedicineId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [StringLength(500)]
    public string? UsageInstruction { get; set; }
}

public class PrescriptionResponseDto
{
    public int Id { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public AppointmentInfoDto Appointment { get; set; } = null!;
    public List<PrescriptionDetailResponseDto> Details { get; set; } = new();
    public DateTime CreationDate { get; set; }
}

public class PrescriptionDetailResponseDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? UsageInstruction { get; set; }
    public MedicineInfoDto Medicine { get; set; } = null!;
}

public class PrescriptionListResponseDto
{
    public int Id { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public AppointmentInfoDto Appointment { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public int DetailCount { get; set; }
}

public class AppointmentInfoDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public PatientInfoDto Patient { get; set; } = null!;
    public DoctorInfoDto Doctor { get; set; } = null!;
}

public class PatientInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class DoctorInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNo { get; set; } = string.Empty;
}

public class MedicineInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
