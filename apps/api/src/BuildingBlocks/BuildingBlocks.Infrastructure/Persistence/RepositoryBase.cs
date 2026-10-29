using System.Linq.Expressions;
using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Repository generic trên EF Core (giống QMS, gọn hơn):
/// audit/xóa mềm/tenant đã do interceptor xử lý nên repository không tự gán CreatedAt, IsDeleted...
/// Global query filter (tenant + soft delete) luôn được áp dụng cho mọi truy vấn qua repository.
/// </summary>
public class RepositoryBase<TEntity, TKey, TContext>(TContext context) : IRepositoryBase<TEntity, TKey>
    where TEntity : EntityBase<TKey>
    where TKey : notnull
    where TContext : DbContext
{
    protected TContext Context { get; } = context;

    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public IQueryable<TEntity> FindAll(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties)
        => BuildQuery(Set.AsNoTracking(), predicate, includeProperties); // Query side: luôn AsNoTracking

    public IQueryable<TEntity> FindAllTracking(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties)
        => BuildQuery(Set.AsTracking(), predicate, includeProperties);

    public Task<TEntity?> FindByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
        => FindAllTracking(null, includeProperties)
            .SingleOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);

    public Task<TEntity?> FindSingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
        => FindAllTracking(null, includeProperties).SingleOrDefaultAsync(predicate, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await Set.AddAsync(entity, cancellationToken);

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        => Set.AddRangeAsync(entities, cancellationToken);

    // Update/Remove CHỈ nhận entity đang được track (đã load qua FindByIdAsync/FindAllTracking,
    // tức là đã qua tenant filter). Entity dựng từ request (Id do client gửi) bị chặn → không thể
    // sửa/xóa dòng của tenant khác. Entity tracked được EF tự phát hiện thay đổi, gọi Update chỉ để tường minh.
    public void Update(TEntity entity) => Set.Update(EnsureTracked(entity));

    public void UpdateRange(IEnumerable<TEntity> entities) => Set.UpdateRange(entities.Select(EnsureTracked));

    /// <summary>Entity ISoftDelete → interceptor chuyển thành xóa mềm; entity khác → xóa cứng.</summary>
    public void Remove(TEntity entity) => Set.Remove(EnsureTracked(entity));

    public void RemoveRange(IEnumerable<TEntity> entities) => Set.RemoveRange(entities.Select(EnsureTracked));

    private TEntity EnsureTracked(TEntity entity) =>
        Context.Entry(entity).State == EntityState.Detached
            ? throw new InvalidOperationException(
                $"{typeof(TEntity).Name} must be loaded with a tracking query before Update/Remove.")
            : entity;

    private static IQueryable<TEntity> BuildQuery(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>[] includeProperties)
    {
        foreach (var include in includeProperties)
            query = query.Include(include);

        return predicate is null ? query : query.Where(predicate);
    }
}

/// <summary>Unit of Work bọc DbContext.SaveChangesAsync (interceptor chạy tại đây).</summary>
public sealed class EfUnitOfWork<TContext>(TContext context) : IUnitOfWork
    where TContext : DbContext
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public void DiscardChanges() => context.ChangeTracker.Clear();
}
