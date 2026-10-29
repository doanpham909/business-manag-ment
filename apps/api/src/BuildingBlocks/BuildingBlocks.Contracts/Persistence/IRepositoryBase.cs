using System.Linq.Expressions;
using BuildingBlocks.Contracts.Domain.Entities;

namespace BuildingBlocks.Contracts.Persistence;

/// <summary>
/// Generic repository (giống QMS, bỏ tham số TContext để Domain/Application không biết DbContext).
/// - FindAll: AsNoTracking → dùng cho query đọc.
/// - FindByIdAsync/FindSingleAsync/FindAllTracking: tracking → dùng khi load-rồi-update.
/// - Update/Remove chỉ nhận entity đang được track (load qua tenant filter) — entity dựng từ request bị chặn.
/// - Remove trên entity ISoftDelete sẽ được interceptor chuyển thành xóa mềm.
/// - KHÔNG gọi SaveChanges trong handler: TransactionPipelineBehavior lo cho mọi command.
/// </summary>
public interface IRepositoryBase<TEntity, in TKey>
    where TEntity : EntityBase<TKey>
    where TKey : notnull
{
    IQueryable<TEntity> FindAll(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties);

    IQueryable<TEntity> FindAllTracking(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties);

    Task<TEntity?> FindByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties);

    Task<TEntity?> FindSingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void UpdateRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);
}

/// <summary>Unit of Work — chỉ TransactionPipelineBehavior (hoặc command [SkipTransaction]) gọi.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Bỏ mọi thay đổi đang track (command thất bại) để không bị lưu nhầm ở lần SaveChanges sau.</summary>
    void DiscardChanges();
}
