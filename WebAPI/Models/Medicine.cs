using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Medicine : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Dosage { get; set; }
    public string? SideEffects { get; set; }

    public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
