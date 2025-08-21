namespace WebAPI.Models.EntityAbstractions;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
    public DateTime? DeletionDate { get; set; }
    public bool IsDeleted { get; set; } = false;
}
