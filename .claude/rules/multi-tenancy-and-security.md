# Multi-tenant, xác thực và phân quyền

## Mô hình tenant

- **Tenant = một công ty khách hàng.** Mọi dữ liệu nghiệp vụ thuộc đúng một tenant.
- Entity nghiệp vụ **bắt buộc** implement `ITenantEntity` (cột `tenant_id uuid not null`).
- `tenant_id` lấy từ claim `tenant_id` trong JWT qua `ITenantContext`. **Không bao giờ** nhận `tenantId` từ body, query string hay header do client gửi.
- Khi insert, interceptor tự gán `TenantId`. Handler không tự set.
- Khi đọc, global query filter của `BmsDbContext` tự lọc theo tenant hiện tại và loại bản ghi đã xóa mềm.
- `IgnoreQueryFilters()` chỉ dùng cho tác vụ cấp hệ thống (quản trị nền tảng, job nền) và phải có comment giải thích. Khi dùng phải tự lọc lại `TenantId` nếu tác vụ vẫn thuộc một tenant.
- Unique index của bảng tenant phải gồm `tenant_id` (ví dụ: `(tenant_id, code)`).
- Truy vấn SQL thô (`FromSql`, Dapper) không có global filter → **bắt buộc** có điều kiện `tenant_id`.

## Người dùng và vai trò

- Mỗi user thuộc một tenant. Riêng **quản trị nền tảng** (platform admin) không thuộc tenant nào, chỉ quản lý danh sách tenant, gói dịch vụ, không xem dữ liệu nghiệp vụ.
- Role do từng tenant tự định nghĩa (role là dữ liệu có `tenant_id`). User ↔ Role là nhiều–nhiều.

## Permission

- Mã permission: `{module}.{resource}.{action}`, chữ thường, ví dụ `inventory.warehouse.view`, `accounting.voucher.approve`.
- Action chuẩn: `view`, `create`, `update`, `delete`, `approve`, `export`. Thêm action khác phải có lý do nghiệp vụ.
- Danh sách permission khai báo tập trung ở backend (một class hằng số cho mỗi module). FE có danh sách tương ứng để ẩn/hiện menu và nút.
- **Backend là nơi kiểm tra quyền thật sự.** Mỗi endpoint (trừ login, health) phải gắn yêu cầu permission. FE ẩn nút chỉ để trải nghiệm tốt hơn.
- Phạm vi dữ liệu (data scope) gắn với role: `own` (do mình tạo/phụ trách), `department` (phòng ban), `all` (toàn công ty). Lọc theo scope làm ở tầng query, không lọc sau khi đã lấy dữ liệu.

## Xác thực

- Access token JWT ngắn hạn (15 phút) + refresh token dài hạn, xoay vòng mỗi lần làm mới, lưu trong cookie `HttpOnly; Secure; SameSite=Strict`.
- Mật khẩu băm bằng thuật toán chậm (ASP.NET Core Identity `PasswordHasher` hoặc Argon2). Không tự viết thuật toán băm.
- Khóa ký JWT, connection string lấy từ cấu hình bí mật, không để trong `appsettings.json`.

## Bảo mật chung

- Validate mọi input bằng FluentValidation ở Application layer.
- Không trả stack trace, câu SQL, tên bảng cho client. Lỗi 500 chỉ trả thông báo chung + `traceId`.
- Không log mật khẩu, token, số tài khoản ngân hàng, dữ liệu cá nhân nhạy cảm.
- Thao tác thay đổi dữ liệu quan trọng (kế toán, kho, phân quyền) phải ghi audit log: ai, khi nào, tenant nào, bản ghi nào, giá trị trước/sau.
