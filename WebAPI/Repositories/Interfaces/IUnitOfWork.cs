using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Repositories.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : Entity;
    Task<int> SaveChangeAsync();
}
