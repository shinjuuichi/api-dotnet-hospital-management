using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class PrescriptionDetail : BaseEntity
{

    public int Quantity { get; set; }

    public string UsageInstruction { get; set; } = string.Empty;

    public int MedicineId { get; set; }

    public int PrescriptionId { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}
