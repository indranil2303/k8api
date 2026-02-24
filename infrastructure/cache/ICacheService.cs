namespace infrastructure.cache;

public interface ICacheService<TEntity> 
where TEntity : class
{
    Task<TEntity?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan ttl);
    Task RemoveAsync(string key);
    Task RemoveAsyncPrefix(string prefix);
}