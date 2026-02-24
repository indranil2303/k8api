using domain.interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace infrastructure.persistence;

public class EfRepository<TEntity, TContext>(TContext context) 
: IRepository<TEntity, TContext> where TEntity 
: class where TContext : LocaldbContext, IDisposable
{
    private readonly TContext _context = context;

    public async Task<TEntity?> GetByIdAsync(Guid id) =>
        await _context.Set<TEntity>().FindAsync(id);

    public async Task<IEnumerable<TEntity>> GetPagedAsync(int pageNumber, int pageSize) => 
        await _context.Set<TEntity>().Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

    public async Task AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public void Dispose() => _context.Dispose();
}