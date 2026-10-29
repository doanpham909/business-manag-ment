# business-manag-ment

Hệ thống quản lý doanh nghiệp: ứng dụng web SaaS cho nhiều công ty (multi-tenant).

**Module:** Quản trị hệ thống · Kho · Kế toán · Quản lý dự án

| Phần | Công nghệ |
|---|---|
| Backend (`apps/api`) | .NET 10, ASP.NET Core Web API, Clean Architecture + DDD + CQRS (MediatR), EF Core 10 |
| Database | PostgreSQL 18 |
| Frontend (`apps/web`) | React 19, TypeScript, Vite, Ant Design 6, `@systemvci/shared-ui`, Tailwind, TanStack Query |

Lý do chọn công nghệ: [docs/architecture/adr-0001-tech-stack-and-multi-tenancy.md](docs/architecture/adr-0001-tech-stack-and-multi-tenancy.md).

## Yêu cầu cài đặt

- .NET SDK 10.0.4xx
- Node.js ≥ 24, npm
- PostgreSQL 18 (cài trực tiếp trên máy)
- Git

## Chạy lần đầu

```bash
git clone https://github.com/doanpham909/business-manag-ment.git
cd business-manag-ment
npm install                      # cài commitlint + bật Git hooks (.husky)

# 1. Database: tạo DB rỗng
psql -U postgres -c "CREATE DATABASE bms_dev;"

# 2. Backend
cd apps/api
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=bms_dev;Username=postgres;Password=<mat-khau>" --project src/Services/Bms/Bms.API
dotnet tool restore
dotnet run --project src/Services/Bms/Bms.API    # http://localhost:5080  (Swagger: /swagger, health: /health)

# 3. Frontend (terminal khác)
cd apps/web
npm install
npm run dev                                      # http://localhost:4200
```

Chi tiết từng phần: [apps/api/README.md](apps/api/README.md), [apps/web/README.md](apps/web/README.md).

## Quy trình làm việc

1. Tạo branch từ `main`: `feat/{module}-{mo-ta}`.
2. Viết đặc tả trong `docs/features/{module}/` → duyệt → code → test (xem [.claude/rules/feature-workflow.md](.claude/rules/feature-workflow.md)).
3. Commit theo Conventional Commits, ví dụ `feat(inventory): add warehouse list api` (hook tự kiểm tra).
4. Mở Pull Request vào `main`; CI (GitHub Actions) phải xanh và có review.

Quy tắc code chi tiết nằm trong [.claude/rules/](.claude/rules/), tóm tắt ở [CLAUDE.md](CLAUDE.md).
