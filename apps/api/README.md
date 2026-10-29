# BMS API — Backend Quản lý doanh nghiệp

ASP.NET Core Web API (.NET 10) · EF Core 10 + PostgreSQL · Clean Architecture + DDD + CQRS (MediatR 12.5, FluentValidation). Multi-tenant: dùng chung database, cột `tenant_id` + global query filter.

## Cấu trúc

```
src/BuildingBlocks/   Contracts (DDD base, abstractions) · Infrastructure (behaviors, interceptors, repository) · Shared (Result, PagedResult, ErrorResponse)
src/Services/Bms/     API · Presentation · Application · Domain · Infrastructure · Persistence
tests/                Bms.UnitTests · Bms.ArchitectureTests (NetArchTest — ép luồng phụ thuộc giữa các tầng)
```

Module nghiệp vụ (Admin, Inventory, Accounting, Projects) đặt theo thư mục `{Module}/` trong từng tầng.

## Yêu cầu

- .NET SDK 10.0.403 (xem `global.json`)
- PostgreSQL 18 chạy local (port 5432)

## Cấu hình connection string (user-secrets)

`appsettings.Development.json` chỉ chứa connection string **không có mật khẩu**. Đặt chuỗi đầy đủ bằng user-secrets (lưu ngoài repo, không bao giờ commit):

```bash
cd src/Services/Bms/Bms.API
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=bms_dev;Username=postgres;Password=<mat-khau>"
dotnet user-secrets list
```

Môi trường khác: dùng biến môi trường `ConnectionStrings__Default`.

## Chạy

```bash
dotnet run --project src/Services/Bms/Bms.API --launch-profile http
```

- API: http://localhost:5080
- Swagger (chỉ Development): http://localhost:5080/swagger
- Health: http://localhost:5080/health (DB không kết nối được → 503 `Unhealthy`, app vẫn chạy)
- Mẫu: `GET http://localhost:5080/api/v1/system/info`

## Build & test

```bash
dotnet restore Bms.slnx
dotnet build Bms.slnx -warnaserror
dotnet test --solution Bms.slnx
```

Test chạy trên Microsoft.Testing.Platform (bật trong `global.json`, mục `test.runner`).

## Migration (EF Core)

```bash
dotnet tool restore   # cài dotnet-ef theo .config/dotnet-tools.json

dotnet ef migrations add <TenMigration> \
  --project src/Services/Bms/Bms.Persistence \
  --startup-project src/Services/Bms/Bms.API \
  --output-dir Migrations

dotnet ef database update \
  --project src/Services/Bms/Bms.Persistence \
  --startup-project src/Services/Bms/Bms.API
```

## Quy ước chính

- Handler **không** tự gọi `SaveChanges` — `TransactionPipelineBehavior` lưu một lần cho mỗi `ICommand`.
- Query đọc dùng `FindAll()` (AsNoTracking); load để sửa dùng `FindByIdAsync()`.
- Id dạng Guid sinh bằng `Guid.CreateVersion7()` trong factory method của entity.
- Entity `ITenantEntity` được gán `TenantId` tự động; muốn đọc chéo tenant (chỉ admin/hệ thống) dùng `IgnoreQueryFilters([QueryFilterNames.Tenant])`.
- `Remove()` trên entity `ISoftDelete` sẽ thành xóa mềm.
- Không dùng AutoMapper / FluentAssertions v8+ / MediatR v13+ (đều là license thương mại).
