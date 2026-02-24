public interface ICacheVersionService<TEntity> where TEntity : class
{
    Task<int> GetVersionAsync(string entity);
    Task IncrementVersionAsync(string entity);
}