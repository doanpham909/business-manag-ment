# Construction ERP — Phân tích Giai đoạn 1

- **Trạng thái:** Bản nháp chờ PO duyệt. Chưa có yêu cầu nào trong bộ tài liệu này được coi là đã duyệt.
- **Ngày:** 29/10/2026
- **Nguồn:** Master Prompt "Construction ERP" v1.0 Draft (6 phân hệ) + hiện trạng repo `business-manag-ment`.

## Nhãn trạng thái dùng trong toàn bộ tài liệu

| Nhãn | Nghĩa |
|---|---|
| **[C]** Confirmed | Có trong master prompt hoặc ADR/rule đã duyệt của repo |
| **[P]** Proposed | Đề xuất của nhóm phân tích, cần PO đồng ý |
| **[A]** Assumed | Giả định để thiết kế tiếp được, cần xác nhận |
| **[U]** Unresolved | Chưa đủ thông tin; có câu hỏi `Q-xx` trong [07-open-questions.md](07-open-questions.md) |

## Mục lục

| # | Tài liệu | Nội dung |
|---|---|---|
| 1 | [01-feature-matrix.md](01-feature-matrix.md) | Feature Matrix 6 phân hệ, ưu tiên, phụ thuộc |
| 2 | [02-sitemap-menu-screens.md](02-sitemap-menu-screens.md) | Sitemap, menu sidebar, danh sách màn hình |
| 3 | [03-data-model.md](03-data-model.md) | Data model cấp logic, nguồn dữ liệu chủ quản |
| 4 | [04-permission-matrix.md](04-permission-matrix.md) | Vai trò, mã quyền, phạm vi dữ liệu, ma trận sơ bộ |
| 5 | [05-integration-matrix.md](05-integration-matrix.md) | Tích hợp và đối soát giữa phân hệ |
| 6 | [06-business-rules.md](06-business-rules.md) | Business rules quan trọng |
| 7 | [07-open-questions.md](07-open-questions.md) | Câu hỏi mở + Decision Log |
| 8 | [08-prototype-plan.md](08-prototype-plan.md) | Kế hoạch prototype theo giai đoạn |

## 1. Hiện trạng repo (kiểm tra ngày 29/10/2026)

| Hạng mục | Hiện trạng |
|---|---|
| Backend | .NET 10, Clean Architecture + DDD + CQRS (MediatR 12), EF Core 10 + PostgreSQL 18. Mới có BuildingBlocks (tenant interceptor, audit interceptor, transaction/validation pipeline, `Result`, `PagedResult`) và endpoint `SystemInfo` |
| Frontend | React 19 + Vite, Ant Design 6 + `@systemvci/shared-ui` + Tailwind, TanStack Query, Formik + Yup, Zustand. Có main layout, sidebar, breadcrumb, router cấu hình; 4 trang placeholder `admin`, `inventory`, `accounting`, `projects` |
| Nghiệp vụ | Chưa có entity, bảng hay API nghiệp vụ nào. Chưa có xác thực (JWT) |
| Tài liệu | ADR-0001 (stack + multi-tenant, **đã duyệt**), bộ rule `.claude/rules/`. Chưa có `docs/features/` |
| Git | 6 commit scaffold trên `main` |

## 2. Điểm master prompt khác repo — đề xuất xử lý

Master prompt nói rõ stack chỉ là đề xuất, cần đối chiếu repo. Repo đã có ADR-0001 được duyệt, nên **mặc định đi theo repo**. Những điểm khác biệt:

| # | Master prompt | Repo hiện tại | Đề xuất |
|---|---|---|---|
| D-01 | NestJS + Prisma | .NET 10 + EF Core (ADR-0001 đã duyệt, NestJS đã bị loại) | **[P]** Giữ .NET. Đổi stack phải viết ADR mới |
| D-02 | shadcn/ui, React Hook Form + Zod | Ant Design 6 + shared-ui, Formik + Yup | **[P]** Giữ theo repo. Các "Shared Components" (AppDataGrid, AppFilterBar…) là wrapper trên shared-ui/antd đặt trong `apps/web/src/shared/components/` |
| D-03 | ECharts, AG Grid | Rule `design-system.md`: không thêm thư viện UI ngoài | Lưới Excel: **[C]** AG Grid Community, chỉ cho màn hình lưới nhập liệu (DEC-007); cần thêm dòng ngoại lệ vào `design-system.md`. Biểu đồ: **[U]** `Q-ARC-03` |
| D-04 | i18n, ưu tiên tiếng Việt | ADR-0001: chỉ tiếng Việt, không dùng i18n framework | **[P]** Giữ chỉ tiếng Việt |
| D-05 | 4 module trong repo | 6 phân hệ | **[C]** Đã thêm `hrm`, `payroll` vào `project-architecture.md` (DEC-014) |
| D-06 | "Company/pháp nhân", "chuyển ngữ cảnh công ty" | Tenant = một công ty khách hàng | **[C]** Đã chốt ở [ADR-0002](../architecture/adr-0002-multi-company-within-tenant.md); rule `multi-tenancy-and-security.md` đã cập nhật |
| D-07 | DataScope 7 mức | Rule: `own`, `department`, `all` | **[C]** Tầng công ty (ADR-0002) + 5 mức trong công ty: `all`, `department`, `department-tree`, `project`, `own` (DEC-015) |
| D-08 | Action quyền: thêm Import, Submit, Reject, Manage | Rule: `view, create, update, delete, approve, export` | **[C]** Thêm `import`, `submit`, `lock`, `view-sensitive`; Reject gộp vào `approve`; bỏ `manage` (DEC-015) |
| D-09 | Thư mục `docs/business`, `docs/modules`… | Rule: đặc tả ở `docs/features/{module}/` | **[P]** Giữ quy ước repo. Thư mục `docs/analysis/` chỉ để chứa bản phân tích này |
| D-10 | Monorepo có `packages/ui`, `packages/types` | Rule: FE và BE không chia sẻ code | **[P]** Không tạo `packages/`. Cần type dùng chung thì sinh TS client từ OpenAPI (ADR-0001) |
| D-11 | Dark Mode | Chưa biết shared-ui có hỗ trợ không | **[U]** → `Q-ARC-05` |

## 3. Ánh xạ phân hệ → mã module

| Phân hệ | Mã module (folder, schema DB, route FE, tiền tố permission) | Có sẵn trong repo |
|---|---|---|
| ADM — Admin | `admin` | Có |
| HRM — Tổ chức – Nhân sự | `hrm` | **Mới** |
| PAY — Chấm công – Tính lương | `payroll` | **Mới** |
| FIN — Tài chính – Kế toán | `accounting` | Có (giữ mã cũ, tên hiển thị đổi thành "Tài chính – Kế toán") |
| WH — Quản lý kho | `inventory` | Có |
| PRJ — Quản lý dự án | `projects` | Có |

## 4. Mô hình công ty **[C]**

Đã chốt ở [ADR-0002](../architecture/adr-0002-multi-company-within-tenant.md). Tóm tắt:

```text
Tenant (khách hàng)  ──1..n──►  Company (pháp nhân)
   │                                  │
   ├─ Cấp tenant: User, Role, Company, danh mục dùng chung (vật tư, NCC, khách hàng, chức danh…)
   └─ UserCompanyAccess + UserRole(company_id): vào công ty nào, vai trò gì ở công ty đó
                                      │
                                      └─ Cấp công ty: phòng ban, nhân viên, dự án, kho, chứng từ,
                                         ngân sách, kỳ lương, kỳ kế toán, workflow…
```

- Đăng nhập → chọn công ty → token mang `tenant_id` + `company_id`. Đổi công ty = token mới.
- Ban lãnh đạo có thêm lựa chọn "Tất cả công ty": chỉ đọc, danh sách/báo cáo có cột Công ty.
- Đổi công ty khi đang mở form: báo người dùng, bỏ dữ liệu chưa lưu, tải lại trang **[P]**.

## 5. Việc cần PO làm để mở Giai đoạn 2

1. Duyệt hoặc sửa phạm vi trong [01-feature-matrix.md](01-feature-matrix.md) (cột Ưu tiên).
2. ✅ D-05, D-06, D-07, D-08 đã duyệt và đã đưa vào rule (DEC-013…015).
3. Các câu hỏi chặn còn lại không chặn GĐ 2.1; trả lời dần theo cột "Điều kiện bắt đầu" ở [08](08-prototype-plan.md) §2.

Sau khi duyệt, mỗi phân hệ sẽ có `docs/features/{module}/0_epic-overview.md` và đặc tả từng chức năng theo `feature-workflow.md`.
