using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class Specialty : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
