---
paths:
  - "apps/api/**"
---

# Quy ước database (PostgreSQL)

## Đặt tên

- Mỗi module một **schema**: `admin`, `inventory`, `accounting`, `projects`. Bảng hạ tầng dùng chung (audit, outbox…) để ở schema `system`.
- Tên bảng: snake_case, số nhiều, ví dụ `inventory.warehouses`, `accounting.journal_entries`. Khai báo qua `ToTable(TableNames.X, TableNames.Schema)` (hằng số tập trung, không viết chuỗi rời rạc).
- Tên cột: snake_case, tự sinh bởi `UseSnakeCaseNamingConvention()`. Không đặt tên cột thủ công trừ khi bắt buộc.
- Khóa ngoại: `{entity}_id`. Index: để EF tự đặt tên, chỉ đặt tay khi cần.

## Cột chuẩn

| Cột | Kiểu | Ghi chú |
|---|---|---|
| `id` | `uuid` | Guid v7 (`Guid.CreateVersion7()`), sắp xếp theo thời gian, tốt cho index |
| `tenant_id` | `uuid not null` | Bảng nghiệp vụ, có index |
| `created_at`, `updated_at` | `timestamptz` | Luôn lưu UTC, interceptor tự gán |
| `created_by`, `updated_by` | `uuid` | ID user, interceptor tự gán |
| `is_deleted`, `deleted_at`, `deleted_by` | | Xóa mềm cho dữ liệu nghiệp vụ |

## Kiểu dữ liệu

- Tiền: `numeric(18,2)`. Số lượng, đơn giá kho: `numeric(18,4)`. Tỷ lệ %: `numeric(9,4)`. **Không dùng** `float`/`double` cho tiền và số lượng.
- Chuỗi: luôn có `HasMaxLength(...)` (mã: 50, tên: 200–255, mô tả: 1000–2000). Văn bản dài: `text`.
- Enum: lưu dạng chuỗi (`HasConversion<string>()`, max length 50) để dễ đọc và an toàn khi thêm giá trị.
- Dữ liệu linh hoạt (cấu hình, thuộc tính mở rộng): `jsonb`, chỉ khi thật cần, không thay cho cột có cấu trúc.
- Ngày không có giờ (ngày chứng từ, ngày hạch toán): `date` (`DateOnly`).

## Ràng buộc và hiệu năng

- Unique theo tenant: `(tenant_id, code)`. Với bảng có xóa mềm, dùng filtered index `WHERE is_deleted = false`.
- Index cho cột hay lọc/sắp xếp, luôn đặt `tenant_id` ở đầu index ghép.
- Chứng từ có thể bị nhiều người sửa cùng lúc (kế toán, kho): bật optimistic concurrency bằng cột hệ thống `xmin` (`IsRowVersion()` với Npgsql).

## Migration

- Chỉ thay đổi schema qua EF Core migration. Không sửa DB bằng tay.
- Tên migration PascalCase mô tả nội dung: `AddInventoryWarehouses`, `AddUnitPriceToStockItems`.
- **Không sửa migration đã merge vào `main`.** Cần đổi thì tạo migration mới.
- Xem lại file migration sinh ra trước khi commit (đặc biệt lệnh drop/rename cột).
- Migration có thay đổi dữ liệu lớn hoặc khóa bảng lâu phải ghi chú trong PR.
