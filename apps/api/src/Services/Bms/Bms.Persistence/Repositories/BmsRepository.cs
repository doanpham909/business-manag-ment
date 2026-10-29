using BuildingBlocks.Contracts.Domain.Entities;
using BuildingBlocks.Infrastructure.Persistence;

namespace Bms.Persistence.Repositories;

/// <summary>
/// Repository generic gắn với <see cref="BmsDbContext"/> — đăng ký open generic cho
/// IRepositoryBase&lt;TEntity, TKey&gt;. Repository đặc thù của aggregate thì kế thừa lớp này.
/// </summary>
public class BmsRepository<TEntity, TKey>(BmsDbContext context)
    : RepositoryBase<TEntity, TKey, BmsDbContext>(context)
    where TEntity : EntityBase<TKey>
    where TKey : notnull;
