# CLAUDE.md

Hệ thống **quản lý doanh nghiệp** dạng SaaS multi-tenant (nhiều công ty dùng chung). Giao diện tiếng Việt.
Module: Quản trị hệ thống (`admin`), Kho (`inventory`), Kế toán (`accounting`), Quản lý dự án (`projects`).

## Repo

```
apps/api/    .NET 10 Web API — Clean Architecture + DDD + CQRS, EF Core 10 + PostgreSQL   (Bms.slnx)
apps/web/    React 19 + TypeScript + Vite — Ant Design 6 + @systemvci/shared-ui + Tailwind
docs/        architecture/ (ADR), features/{module}/ (đặc tả chức năng)
.claude/rules/  Quy tắc chi tiết — ĐỌC file liên quan trước khi code
```

## Lệnh thường dùng

```bash
# Backend (cd apps/api)
dotnet build Bms.slnx -warnaserror
dotnet test Bms.slnx
dotnet run --project src/Services/Bms/Bms.API          # http://localhost:5080, Swagger /swagger
dotnet format Bms.slnx                                  # sửa định dạng trước khi commit

# Frontend (cd apps/web)
npm run dev                                             # http://localhost:4200, proxy /api → :5080
npm run lint && npm run typecheck && npm run test:run && npm run build
```

Migration và cấu hình kết nối DB: xem `apps/api/README.md`.

## Quy tắc (`.claude/rules/`)

| File | Khi nào đọc |
|---|---|
| `project-architecture.md` | Luôn luôn — stack, module, ranh giới |
| `feature-workflow.md` | Bắt đầu một chức năng mới |
| `multi-tenancy-and-security.md` | Mọi thay đổi chạm dữ liệu, API, quyền |
| `backend-architecture.md`, `backend-implement-steps.md` | Code trong `apps/api` |
| `efcore-postgresql-rules.md`, `database-conventions.md` | Viết query/command, entity, migration |
| `api-conventions.md` | Thêm/sửa endpoint hoặc gọi API từ FE |
| `frontend-implement-rules.md`, `design-system.md` | Code trong `apps/web` |
| `testing-rules.md` | Viết test, trước khi tạo PR |
| `git-commit-rules.md` | Branch, commit, PR |

## Điều bắt buộc

1. **Đặc tả trước, code sau.** Chức năng mới phải có đặc tả trong `docs/features/{module}/` được người dùng duyệt (xem `feature-workflow.md`).
2. **Không rò rỉ dữ liệu giữa tenant.** Entity nghiệp vụ implement `ITenantEntity`; không nhận `tenantId` từ client; SQL thô phải lọc `tenant_id`.
3. **Handler không gọi `SaveChangesAsync`.** `TransactionPipelineBehavior` lo.
4. **FE dùng shared-ui trước antd**, gọi API qua `apiClient`, server state qua TanStack Query.
5. **Không sửa migration đã merge, không nới lỏng architecture test, không skip test để CI xanh.**
6. **Không commit bí mật.** Connection string có mật khẩu đặt trong `dotnet user-secrets`; FE dùng `.env.local`.
7. Không dùng MediatR ≥ 13, AutoMapper ≥ 15, FluentAssertions ≥ 8 (license thương mại).
8. Không tham chiếu dự án cũ `business-manag-ment-main`. QMS chỉ để tham khảo.

## Cách làm việc

- Nêu giả định khi chưa rõ yêu cầu; có nhiều cách hiểu thì hỏi, không tự chọn.
- Sửa đúng phạm vi được yêu cầu; không "tiện tay" refactor code xung quanh.
- Làm xong phải tự kiểm chứng: build + test + lint xanh, chạy thử trên giao diện nếu có UI. Báo cáo đúng kết quả, kể cả phần chưa làm được.
