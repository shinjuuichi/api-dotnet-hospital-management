using WebAPI.Data;
using WebAPI.Models.EntityAbstractions;
using WebAPI.Repositories.Interfaces;

namespace WebAPI.Repositories.Implements;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, dynamic> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : Entity
    {
        var type = typeof(T);

        if (_repositories.ContainsKey(type))
        {
            return _repositories[type];
        }

        var repository = new GenericRepository<T>(_context);
        _repositories.Add(type, repository);

        return repository;
    }

    public async Task<int> SaveChangeAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
