using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class PrescriptionDetail : BaseEntity
{
    public string Quantity { get; set; } = string.Empty;
    
    public string? UsageInstruction { get; set; }

    public int MedicineId { get; set; }

    public int PrescriptionId { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}
