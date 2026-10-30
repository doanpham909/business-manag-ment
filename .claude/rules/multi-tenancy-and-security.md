# Multi-tenant, xác thực và phân quyền

## Mô hình tenant

- **Tenant = một khách hàng** (một công ty hoặc một nhóm công ty — xem mục Công ty bên dưới). Mọi dữ liệu nghiệp vụ thuộc đúng một tenant.
- Entity nghiệp vụ **bắt buộc** implement `ITenantEntity` (cột `tenant_id uuid not null`).
- `tenant_id` lấy từ claim `tenant_id` trong JWT qua `ITenantContext`. **Không bao giờ** nhận `tenantId` từ body, query string hay header do client gửi.
- Khi insert, interceptor tự gán `TenantId`. Handler không tự set.
- Khi đọc, global query filter của `BmsDbContext` tự lọc theo tenant hiện tại và loại bản ghi đã xóa mềm.
- `IgnoreQueryFilters()` chỉ dùng cho tác vụ cấp hệ thống (quản trị nền tảng, job nền) và phải có comment giải thích. Khi dùng phải tự lọc lại `TenantId` nếu tác vụ vẫn thuộc một tenant.
- Unique index của bảng tenant phải gồm `tenant_id` (ví dụ: `(tenant_id, code)`).
- Truy vấn SQL thô (`FromSql`, Dapper) không có global filter → **bắt buộc** có điều kiện `tenant_id`.

## Công ty (pháp nhân) trong tenant — ADR-0002

- Một tenant (khách hàng, nhóm công ty) có nhiều **công ty**. Dữ liệu nghiệp vụ tách theo công ty.
- Mỗi entity nghiệp vụ thuộc **đúng một cấp**, ghi rõ trong đặc tả:
  - **Cấp tenant** (`ITenantEntity`): Company, User, Role, danh mục dùng chung (vật tư, nhà cung cấp, khách hàng, chức danh, danh mục ADM).
  - **Cấp công ty** (`ICompanyEntity : ITenantEntity`, thêm `company_id uuid not null`): phòng ban, nhân viên, dự án, kho, mọi chứng từ, kỳ công/lương/kế toán, ngân sách, workflow.
- `company_id` lấy từ token qua `ICompanyContext`, **không bao giờ** nhận từ client. Interceptor tự gán khi insert; global filter `Company` tự lọc khi đọc.
- Đăng nhập không nhập mã công ty (username unique toàn nền tảng); sau đăng nhập user chọn công ty, đổi công ty = server cấp token mới.
- Chế độ **"Tất cả công ty"** (quyền `admin.company.view-all`, mặc định Ban lãnh đạo) **chỉ đọc**: interceptor chặn ghi `ICompanyEntity` khi không có công ty hiện hành cụ thể.
- Unique index: cấp tenant `(tenant_id, code)`; cấp công ty `(tenant_id, company_id, code)`.
- Tắt filter `Company` theo đúng quy định của `IgnoreQueryFilters()` ở trên. SQL thô với bảng cấp công ty **bắt buộc** điều kiện `company_id`.
- Mỗi chức năng cấp công ty phải có test với 2 công ty cùng tenant (không đọc/sửa chéo; xem gộp đọc được nhưng không ghi được).

## Người dùng và vai trò

- Mỗi user thuộc một tenant. Riêng **quản trị nền tảng** (platform admin) không thuộc tenant nào, chỉ quản lý danh sách tenant, gói dịch vụ, không xem dữ liệu nghiệp vụ.
- User được vào những công ty khai báo trong `UserCompanyAccess`. Quản trị tenant tạo công ty mới và gán user vào công ty.
- Role do từng tenant tự định nghĩa (role là dữ liệu có `tenant_id`). Gán role cho user **theo từng công ty** (`UserRole` có `company_id`); quyền hiệu lực = các role của user tại công ty hiện hành.
- User liên kết tối đa một nhân viên; một nhân viên thuộc đúng một công ty và không điều chuyển sang công ty khác (`CompanyId` của nhân viên không bao giờ đổi).
- Hệ thống tài khoản kế toán là dữ liệu **cấp tenant**, dùng chung mọi công ty.

## Permission

- Mã permission: `{module}.{resource}.{action}`, chữ thường, ví dụ `inventory.warehouse.view`, `accounting.voucher.approve`.
- Action chuẩn: `view`, `create`, `update`, `delete`, `approve`, `export`, `import`, `submit`, `lock`, `view-sensitive`. Thêm action khác phải có lý do nghiệp vụ.
  - `approve` gồm cả từ chối ở cùng bước duyệt. Người duyệt còn phải đúng người/vai trò của bước hiện tại trong workflow.
  - `import`: ghi hàng loạt từ Excel (tách khỏi `create`). `submit`: gửi duyệt. `lock`: khóa/mở khóa kỳ (bảng công, bảng lương, kỳ kế toán, kiểm kê).
  - `view-sensitive`: xem trường nhạy cảm (số định danh, tài khoản ngân hàng, lương). Không có quyền này thì API **không trả** trường đó (kể cả khi export), không chỉ ẩn ở FE.
- Danh sách permission khai báo tập trung ở backend (một class hằng số cho mỗi module). FE có danh sách tương ứng để ẩn/hiện menu và nút.
- **Backend là nơi kiểm tra quyền thật sự.** Mỗi endpoint (trừ login, health) phải gắn yêu cầu permission. FE ẩn nút chỉ để trải nghiệm tốt hơn.
- Phạm vi dữ liệu (data scope) gắn với role **theo từng module**, áp **bên trong công ty hiện hành**:
  - `all`: toàn công ty.
  - `department`: đúng phòng ban của người dùng.
  - `department-tree`: phòng ban của người dùng và mọi đơn vị cấp dưới.
  - `project`: các dự án được gán cho role/user.
  - `own`: bản ghi do mình tạo hoặc mình là người phụ trách/được phân công.
- Phòng ban của người dùng lấy từ phân công hiện hành của nhân viên liên kết; user chưa liên kết nhân viên → scope `department`/`department-tree` ra rỗng. Nhiều role → hợp các phạm vi.
- Lọc theo scope làm ở tầng query, không lọc sau khi đã lấy dữ liệu. Áp như nhau cho danh sách, chi tiết, export, báo cáo.
- Bản ghi có nhiều thuộc tính phạm vi (vd. cả dự án và phòng ban) thuộc scope khi khớp **một** trong các thuộc tính đó.

## Xác thực

- Access token JWT ngắn hạn (15 phút) + refresh token dài hạn, xoay vòng mỗi lần làm mới, lưu trong cookie `HttpOnly; Secure; SameSite=Strict`.
- Mật khẩu băm bằng thuật toán chậm (ASP.NET Core Identity `PasswordHasher` hoặc Argon2). Không tự viết thuật toán băm.
- Khóa ký JWT, connection string lấy từ cấu hình bí mật, không để trong `appsettings.json`.

## Bảo mật chung

- Validate mọi input bằng FluentValidation ở Application layer.
- Không trả stack trace, câu SQL, tên bảng cho client. Lỗi 500 chỉ trả thông báo chung + `traceId`.
- Không log mật khẩu, token, số tài khoản ngân hàng, dữ liệu cá nhân nhạy cảm.
- Thao tác thay đổi dữ liệu quan trọng (kế toán, kho, phân quyền) phải ghi audit log: ai, khi nào, tenant nào, bản ghi nào, giá trị trước/sau.
