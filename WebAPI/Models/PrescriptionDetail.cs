using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class PrescriptionDetail : BaseEntity
{
    public string Dosage { get; set; } = string.Empty;
    
    public string Frequency { get; set; } = string.Empty;

    public int Duration { get; set; }

    public string? Instructions { get; set; }

    public int MedicineId { get; set; }

    public int PrescriptionId { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}
