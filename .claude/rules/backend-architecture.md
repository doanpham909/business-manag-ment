---
paths:
  - "apps/api/**"
---

# Kiến trúc backend (.NET 10)

Clean Architecture + DDD + CQRS, mô phỏng QMS. Solution: `apps/api/Bms.slnx`.

## Layer và chiều phụ thuộc

```
src/BuildingBlocks/
├── BuildingBlocks.Shared/          Result, Error, PagedResult, ErrorResponse, ErrorCodes, AppConstants — KHÔNG phụ thuộc gì
├── BuildingBlocks.Contracts/       EntityBase, EntityAuditBase, EventEntityAuditBase, ITenantEntity, IAggregateRoot,
│                                   DomainEvent, IBusinessRule, ValueObject, exceptions, ICommand/IQuery, IRepositoryBase, IUnitOfWork,
│                                   ICurrentUser, ITenantContext
└── BuildingBlocks.Infrastructure/  Pipeline behaviors, EF interceptors, RepositoryBase, TenantDbContextBase, GlobalExceptionHandler
src/Services/Bms/
├── Bms.Domain/          Aggregate, entity, value object, rule, event, exception — theo module
├── Bms.Application/     Command/Query + handler + validator + response DTO
├── Bms.Presentation/    Controller (mỏng)
├── Bms.Infrastructure/  Dịch vụ ngoài: identity từ HttpContext, email, file, cache…
├── Bms.Persistence/     BmsDbContext, IEntityTypeConfiguration, repository đặc thù, migration
└── Bms.API/             Program.cs, DI, middleware, appsettings
```

- Shared ← Contracts ← Domain ← Application ← Presentation ← API.
- Persistence → Domain + BuildingBlocks.Infrastructure. Infrastructure → Contracts.
- Application được dùng `Microsoft.EntityFrameworkCore` (chỉ async LINQ), **không** được dùng ASP.NET Core hay Persistence.
- `tests/Bms.ArchitectureTests` kiểm tra các quy tắc trên. **Không nới lỏng test kiến trúc để code qua được**; sửa code cho đúng layer.

## Module

Mỗi layer chia folder theo module: `Admin`, `Inventory`, `Accounting`, `Projects` (xem `project-architecture.md`).

```
Bms.Domain/{Module}/{Aggregate}/            {Aggregate}.cs, Entities/, Events/, Rules/, Enums/, Exceptions/, ValueObjects/
Bms.Application/{Module}/{Aggregate}/       Commands/{UseCase}/, Queries/{UseCase}/
Bms.Persistence/Configurations/{Module}/    {Entity}Configuration.cs
Bms.Persistence/Constants/TableNames.cs     Tên schema + bảng
Bms.Presentation/Controllers/V1/{Module}/   {Aggregate}Controller.cs
```

## Domain (không anemic model)

- Aggregate root: `EventEntityAuditBase<Guid>, IAggregateRoot` (+ `ITenantEntity` nếu là dữ liệu nghiệp vụ). Entity con: `EntityAuditBase<Guid>`.
- Setter `private`, constructor rỗng `private` cho EF, tạo mới qua **factory** `static Create(...)`, thay đổi qua method nghiệp vụ (`Update`, `Approve`, `Cancel`…).
- Id: `Guid.CreateVersion7()` trong factory.
- Business rule: `{Subject}Must{Condition}Rule : IBusinessRule`, gọi `CheckRule(...)`. Vi phạm → 422.
- Domain event: `{Aggregate}{Action}Event` (thì quá khứ), phát trong method nghiệp vụ.
- Mỗi aggregate có `{Aggregate}NotFoundException : NotFoundException`.
- Tham chiếu aggregate khác bằng Id, không bằng navigation. Một command chỉ sửa một aggregate.

## Pipeline (tự động, không viết lại trong handler)

Thứ tự: `RequestLogging` → `Validation` (FluentValidation, lỗi → 400) → `Transaction` (command thành công → `SaveChangesAsync` một lần).

- **Handler không gọi `SaveChangesAsync`.** Ngoại lệ: command gắn `[SkipTransaction]`, phải có comment lý do.
- Command thất bại (`Result.Failure`) → pipeline tự bỏ mọi thay đổi đang track.
- Interceptor tự gán `CreatedAt/By`, `UpdatedAt/By`, `TenantId`, đổi `Remove` thành xóa mềm (chỉ ghi cột `is_deleted/deleted_*`), phát domain event sau khi lưu.

## Ràng buộc an toàn khi ghi dữ liệu

- **Chỉ sửa/xóa entity đã load bằng query tracking** (`FindByIdAsync`, `FindAllTracking`). `Update`/`Remove` entity dựng từ request sẽ bị chặn. Đây là hàng rào chống sửa dữ liệu của tenant khác.
- **Handler không `Send` command khác** (mỗi command lưu riêng → mất tính nguyên tử). Dùng chung logic thì tách thành domain service hoặc method dùng chung.
- **Domain event handler không ghi vào DbContext** (chạy sau khi đã lưu, thay đổi sẽ không được lưu). Chỉ dùng cho tác vụ phụ: gửi thông báo, ghi log. Cần ghi dữ liệu theo sự kiện thì bàn thiết kế outbox trước.
- **Không dùng `ExecuteUpdate`/`ExecuteDelete`** trên entity có `ISoftDelete`/`IAuditable` (bỏ qua xóa mềm, audit, kiểm tra tenant). Cần cập nhật hàng loạt thì nêu trong đặc tả để review.
- `BmsDbContext` đăng ký bằng `AddDbContext`, **không** dùng `AddDbContextPool` (context giữ tenant của request đầu → rò rỉ).

## Lỗi

| Tình huống | Cách làm | HTTP |
|---|---|---|
| Input sai định dạng | FluentValidation validator | 400 |
| Vi phạm quy tắc nghiệp vụ | `CheckRule(...)` trong domain | 422 |
| Không tìm thấy | `throw new {Aggregate}NotFoundException(id)` | 404 |
| Trùng dữ liệu | class riêng kế thừa `ConflictException` | 409 |
| Không có quyền | class riêng kế thừa `AccessDeniedException` | 403 |
| Lỗi mong đợi khác | `return Result.Failure(error)` | 400 |

Message lỗi trả người dùng bằng tiếng Việt. Không `catch` rồi nuốt lỗi; để `GlobalExceptionHandler` xử lý.

## Không dùng

MediatR ≥ 13, AutoMapper ≥ 15, FluentAssertions ≥ 8 (license thương mại). Mapping bằng tay hoặc projection `Select(...)`.
