# Kiến trúc tổng thể

## Stack

| Phần | Công nghệ |
|---|---|
| Frontend (`apps/web`) | React + TypeScript + Vite, Ant Design 6 + `@systemvci/shared-ui` + Tailwind, TanStack Query + axios, Formik + Yup, Zustand, React Router |
| Backend (`apps/api`) | .NET 10, ASP.NET Core Web API, Clean Architecture + DDD + CQRS (MediatR), FluentValidation, Serilog |
| Database | PostgreSQL 18, EF Core 10 (Npgsql), tên bảng/cột snake_case |
| CI | GitHub Actions (`.github/workflows/ci.yml`) |

Kiến trúc mô phỏng dự án QMS của công ty để đội dùng lại kinh nghiệm. Khác biệt chính: PostgreSQL thay SQL Server, .NET 10 thay .NET 8, multi-tenant từ đầu.

## Bản chất hệ thống

- **SaaS multi-tenant**: nhiều công ty dùng chung một database, cách ly bằng cột `tenant_id`. Xem `multi-tenancy-and-security.md`.
- **Modular monolith**: một API, chia module nghiệp vụ. Module hiện có:

| Module | Mã (code, folder, schema DB, route FE) | Tên hiển thị |
|---|---|---|
| Quản trị hệ thống | `admin` | Quản trị hệ thống |
| Tổ chức – Nhân sự | `hrm` | Tổ chức – Nhân sự |
| Chấm công – Tính lương | `payroll` | Chấm công – Tính lương |
| Tài chính – Kế toán | `accounting` | Tài chính – Kế toán |
| Kho | `inventory` | Quản lý kho |
| Quản lý dự án | `projects` | Quản lý dự án |

Thêm module mới phải cập nhật bảng này.

## Quy tắc ranh giới

- Module không truy cập trực tiếp bảng/entity của module khác. Tham chiếu chéo bằng ID; cần dữ liệu module khác thì gọi qua query/service mà module đó công khai.
- `apps/web` và `apps/api` chỉ giao tiếp qua REST API (`/api/v1/...`). Không chia sẻ code giữa hai app.
- Không tham chiếu, import, copy nguyên khối từ dự án cũ `business-manag-ment-main`. QMS chỉ là tài liệu tham khảo, không phải dependency.

## Nguyên tắc chung

- YAGNI, KISS, DRY. Không làm tính năng, abstraction, cấu hình "để dành".
- File code ≤ 200 dòng; vượt thì tách theo trách nhiệm.
- Tên file: C# theo PascalCase (chuẩn .NET), FE theo kebab-case.
- Giao diện, thông báo lỗi trả về người dùng: tiếng Việt. Code, tên biến, commit message: tiếng Anh.
- Không commit bí mật (`.env`, connection string có mật khẩu, khóa JWT). Dùng `dotnet user-secrets` và `.env.local`.
