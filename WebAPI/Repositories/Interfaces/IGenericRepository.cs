using System.Linq.Expressions;
using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Repositories.Interfaces;

public interface IGenericRepository<T> where T : Entity
{
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string[]? includes = null);
    IQueryable<T> GetAllQueryable(string[]? includes = null);
    Task<T?> GetByIdAsync(int id, string[]? includes = null);
    Task<T?> GetByConditionAsync(Expression<Func<T, bool>> filter, string[]? includes = null);
    Task<T> AddAsync(T entity);
    T Update(T entity);
    void Remove(T entity);
    Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null);
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
}
