using infrastructure.persistence;

namespace domain.interfaces;

public interface IRepository<TEntity, TContext> 
where TEntity : class
where TContext : LocaldbContext
{
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> GetPagedAsync(int pageNumber, int pageSize);
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task RemoveAsync(Guid id);
}   