using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebAPI.Data;
using WebAPI.Models.EntityAbstractions;
using WebAPI.Repositories.Interfaces;

namespace WebAPI.Repositories.Implements;

public class GenericRepository<T> : IGenericRepository<T> where T : Entity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !((AuditableEntity)(object)e).IsDeleted);
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync();
    }

    public IQueryable<T> GetAllQueryable(string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !((AuditableEntity)(object)e).IsDeleted);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return query;
    }

    public async Task<T?> GetByIdAsync(int id, string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((BaseEntity)(object)e).Id == id);
        }

        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !((AuditableEntity)(object)e).IsDeleted);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<T?> GetByConditionAsync(Expression<Func<T, bool>> filter, string[]? includes = null)
    {
        IQueryable<T> query = _dbSet;

        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !((AuditableEntity)(object)e).IsDeleted);
        }

        query = query.Where(filter);

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public T Update(T entity)
    {
        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            ((AuditableEntity)(object)entity).ModificationDate = DateTime.UtcNow;
        }

        _dbSet.Update(entity);
        return entity;
    }

    public void Remove(T entity)
    {
        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            var auditableEntity = (AuditableEntity)(object)entity;
            auditableEntity.IsDeleted = true;
            auditableEntity.DeletionDate = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbSet;

        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !((AuditableEntity)(object)e).IsDeleted);
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbSet;

        if (typeof(AuditableEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !((AuditableEntity)(object)e).IsDeleted);
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return await query.CountAsync();
    }
}
