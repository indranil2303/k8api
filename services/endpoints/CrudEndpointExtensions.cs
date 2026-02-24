using System.Text.Json;
using domain.interfaces;
using infrastructure.cache;
using infrastructure.persistence;

public static class CrudEndpointExtensions
{
      public static RouteGroupBuilder MapCrudEndpoints<TEntity, TContext>(
          this IEndpointRouteBuilder routes,
          string routePrefix)
        where TEntity : class, new()
        where TContext : LocaldbContext
    {
          var prefix = (routePrefix ?? string.Empty).Trim('/');
          var group = routes.MapGroup($"/{prefix}");
        
        group.MapGet("/{id:guid}", async (
            IRepository<TEntity, TContext> repo,
            ICacheService<TEntity> cache,
            ICacheVersionService<TEntity> ver,
            Guid id) =>
        {
            var entityName = typeof(TEntity).Name;
            var cacheKey = $"{entityName}:ver{await ver.GetVersionAsync(entityName)}:{id}";

            var cached = await cache.GetAsync(cacheKey);
            if (cached != null)
                return Results.Ok(cached);

            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return Results.NotFound();

            await cache.SetAsync(cacheKey, JsonSerializer.Serialize(entity), TimeSpan.FromMinutes(2));
            return Results.Ok(entity);
        });

        group.MapGet("/", async (
            IRepository<TEntity, TContext> repo,
            ICacheService<TEntity> cache,
            ICacheVersionService<TEntity> ver,
            int page = 1,
            int size = 20) =>
        {
            var entity = typeof(TEntity).Name;
            var key = $"{entity}:ver{await ver.GetVersionAsync(entity)}:list:{page}:{size}";

            var cached = await cache.GetAsync(key);
            if (cached != null)
                return Results.Ok(cached);

            var data = await repo.GetPagedAsync(page, size);

            await cache.SetAsync(key, JsonSerializer.Serialize(data), TimeSpan.FromMinutes(1));
            return Results.Ok(data);
        });

        group.MapPost("/", async (
            IRepository<TEntity, TContext> repo,
            ICacheService<TEntity> cache,
            ICacheVersionService<TEntity> ver,
            TEntity entity) =>
        {
            await repo.AddAsync(entity);

            await ver.IncrementVersionAsync($"{typeof(TEntity).Name}");

            var idProp = typeof(TEntity).GetProperty("Id") ?? typeof(TEntity).GetProperty("id");
            var idValue = idProp?.GetValue(entity)?.ToString() ?? string.Empty;
            return Results.Created($"/{prefix}/{idValue}", entity);
        });

        group.MapPut("/{id:guid}", async (
            IRepository<TEntity, TContext> repo,
            ICacheService<TEntity> cache,
            ICacheVersionService<TEntity> ver,
            Guid id,
            TEntity updatedEntity) =>
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing == null) return Results.NotFound();

            await repo.UpdateAsync(updatedEntity);

            await ver.IncrementVersionAsync($"{typeof(TEntity).Name}");
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (
            IRepository<TEntity, TContext> repo,
            ICacheService<TEntity> cache,
            ICacheVersionService<TEntity> ver,
            Guid id) =>
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing == null) return Results.NotFound();

            await repo.RemoveAsync(id);

            await ver.IncrementVersionAsync($"{typeof(TEntity).Name}");
            return Results.NoContent();
        });

        return group;
    }
}