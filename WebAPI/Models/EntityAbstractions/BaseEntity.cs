using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.EntityAbstractions;

public abstract class BaseEntity : Entity
{
    [Key]
    public int Id { get; set; }
}
