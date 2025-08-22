using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

public class PrescriptionDto
{
    public int Id { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string? Notes { get; set; }
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public List<PrescriptionDetailDto> PrescriptionDetails { get; set; } = new();
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
}

public class CreatePrescriptionDto
{
    [Required]
    public int AppointmentId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public List<CreatePrescriptionDetailDto> PrescriptionDetails { get; set; } = new();
}

public class PrescriptionDetailDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string? Instructions { get; set; }
}

public class CreatePrescriptionDetailDto
{
    [Required]
    public int MedicineId { get; set; }

    [Required]
    [StringLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Frequency { get; set; } = string.Empty;

    [Required]
    [Range(1, 365)]
    public int Duration { get; set; }

    [StringLength(500)]
    public string? Instructions { get; set; }
}