---
paths:
  - "apps/api/**"
---

# Các bước implement API

Thứ tự **bắt buộc**: Domain → Persistence (+ migration) → Application → Presentation → Test.
Ví dụ dưới dùng aggregate `Warehouse` của module `Inventory`. Mẫu chạy được hiện có: `Bms.Application/SystemInfo/` + `SystemController`.

## 1. Domain — `Bms.Domain/Inventory/Warehouses/`

```csharp
public sealed class Warehouse : EventEntityAuditBase<Guid>, IAggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }          // interceptor tự gán
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private Warehouse() { }                              // EF Core

    public static Warehouse Create(string code, string name)
    {
        CheckRule(new WarehouseCodeMustNotBeEmptyRule(code));
        var warehouse = new Warehouse { Id = Guid.CreateVersion7(), Code = code.Trim(), Name = name.Trim() };
        warehouse.AddDomainEvent(new WarehouseCreatedEvent(warehouse.Id));
        return warehouse;
    }
}
```

Kèm theo: `Rules/WarehouseCodeMustNotBeEmptyRule.cs`, `Events/WarehouseCreatedEvent.cs`, `Exceptions/WarehouseNotFoundException.cs` (`: NotFoundException`), `Exceptions/WarehouseCodeAlreadyExistsException.cs` (`: ConflictException`). Các exception gốc đều `abstract`, mỗi tình huống phải có class riêng.

## 2. Persistence — `Bms.Persistence/`

- `Constants/TableNames.cs`: `public const string InventorySchema = "inventory"; public const string Warehouses = "warehouses";`
- `Configurations/Inventory/WarehouseConfiguration.cs`:

```csharp
internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable(TableNames.Warehouses, TableNames.InventorySchema);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("is_deleted = false");
    }
}
```

- Tạo migration (từ `apps/api`), xem lại file sinh ra trước khi commit:

```bash
dotnet ef migrations add AddInventoryWarehouses --project src/Services/Bms/Bms.Persistence --startup-project src/Services/Bms/Bms.API --output-dir Migrations
```

## 3. Application — `Bms.Application/Inventory/Warehouses/`

```
Commands/CreateWarehouse/  CreateWarehouseCommand.cs, CreateWarehouseCommandHandler.cs, CreateWarehouseCommandValidator.cs
Queries/GetWarehouses/     GetWarehousesQuery.cs (+ WarehouseResponse), GetWarehousesQueryHandler.cs
```

```csharp
public sealed record CreateWarehouseCommand(string Code, string Name) : ICommand<Guid>;

internal sealed class CreateWarehouseCommandHandler(IRepositoryBase<Warehouse, Guid> repository)
    : ICommandHandler<CreateWarehouseCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        if (await repository.FindAll(x => x.Code == request.Code).AnyAsync(cancellationToken))
            throw new WarehouseCodeAlreadyExistsException(request.Code);   // kế thừa ConflictException → 409

        var warehouse = Warehouse.Create(request.Code, request.Name);
        await repository.AddAsync(warehouse, cancellationToken);
        return Result.Success(warehouse.Id);                    // KHÔNG SaveChanges
    }
}
```

- Handler: `internal sealed` (architecture test bắt buộc). Command/Query/Response: `public sealed record`.
- Query danh sách: `FindAll(...)` → `Where` → `OrderBy` → `Select(x => new WarehouseResponse(...))` → `ToPagedResultAsync(pageIndex, pageSize, ct)`.
- Validator: `AbstractValidator<CreateWarehouseCommand>`, message tiếng Việt.

## 4. Presentation — `Bms.Presentation/Controllers/V1/Inventory/WarehouseController.cs`

```csharp
[Route($"{AppConstants.ApiV1Prefix}/inventory/warehouses")]
public sealed class WarehouseController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    public Task<IActionResult> Create([FromBody] CreateWarehouseCommand command, CancellationToken ct)
        => SendAsync(command, ct);
}
```

Controller mỏng: chỉ nhận request và `SendAsync`. Tên class số ít (`WarehouseController`), route số nhiều. Gắn permission khi module Auth đã có (xem `multi-tenancy-and-security.md`).

## 5. Test — `tests/Bms.UnitTests/{Module}/`

Factory + rule của aggregate, validator, handler có rẽ nhánh. Chạy `dotnet build Bms.slnx -warnaserror` và `dotnet test Bms.slnx`.
