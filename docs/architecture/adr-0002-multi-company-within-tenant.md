# ADR-0002: Nhiều công ty (pháp nhân) trong một tenant

- **Trạng thái:** Đã duyệt
- **Ngày:** 29/10/2026
- **Bổ sung cho:** ADR-0001 (mục Multi-tenant). ADR-0001 vẫn hiệu lực, ADR này thêm một tầng bên dưới tenant.
- **Quyết định nghiệp vụ gốc:** DEC-003, DEC-005, DEC-008…DEC-012 trong `docs/analysis/07-open-questions.md`

## Bối cảnh

ADR-0001 coi **tenant = một công ty khách hàng**. Khách hàng mục tiêu là doanh nghiệp xây dựng có công ty mẹ và các công ty con dùng chung một hệ thống. PO đã quyết định:

| Quyết định | Nội dung |
|---|---|
| DEC-003 | Một tenant có nhiều pháp nhân; dữ liệu nghiệp vụ tách theo từng công ty |
| DEC-005 | Đăng nhập không nhập mã công ty; sau khi đăng nhập người dùng chọn công ty để làm việc |
| DEC-008 | Danh mục dùng chung cả tập đoàn (không tách theo công ty) |
| DEC-009 | Một nhân viên chỉ thuộc một công ty tại một thời điểm |
| DEC-010 | Bản ghi thuộc phạm vi dữ liệu khi khớp **một** trong các thuộc tính phạm vi (dự án hoặc phòng ban…) |
| DEC-011 | Ban lãnh đạo được xem dữ liệu của tất cả công ty |
| DEC-012 | Quản trị của tenant được tạo công ty mới |

## Quyết định

### 1. Hai tầng cách ly: tenant → company

```text
Tenant (khách hàng, nhóm công ty)        ← cách ly tuyệt đối, như ADR-0001
 ├─ Dữ liệu cấp tenant  : ITenantEntity            (danh mục, user, role, company…)
 └─ Company (pháp nhân) 1..n
     └─ Dữ liệu cấp công ty : ICompanyEntity       (chứng từ, kho, dự án, nhân viên…)
```

- `ICompanyEntity : ITenantEntity` thêm `CompanyId` (`company_id uuid not null`). Entity cấp công ty vẫn có `tenant_id`; hai filter độc lập, cùng áp dụng.
- Tenant là ranh giới bảo mật cứng (giữa các khách hàng). Công ty là ranh giới **nghiệp vụ** trong cùng khách hàng: vẫn bắt buộc lọc, nhưng được phép đọc nhiều công ty khi người dùng có quyền (§4).

### 2. Phân loại dữ liệu

| Cấp tenant (`ITenantEntity`) | Cấp công ty (`ICompanyEntity`) |
|---|---|
| Company · User · Role · UserCompanyAccess · UserRole · Hệ thống tài khoản kế toán | Department (cơ cấu tổ chức) · Employee + phân công |
| Danh mục dùng chung ADM-05 · Vật tư · Nhà cung cấp · Khách hàng · Vị trí/chức danh (DEC-008) | Project · Warehouse · mọi chứng từ kho |
| Cấu hình hệ thống cấp tenant | Ca, lịch làm việc, chấm công, kỳ công, kỳ lương, bảng lương, phiếu lương |
| AuditLog, LoginHistory (có thêm cột `company_id` nullable để lọc) | Ngân sách, chi phí, đề nghị thanh toán, tạm ứng, công nợ, doanh thu, dòng tiền, chứng từ kế toán, kỳ kế toán |
| | Workflow definition (người duyệt khác nhau giữa các công ty), thông báo |

Hệ thống tài khoản kế toán là dữ liệu **cấp tenant**, dùng chung mọi công ty (DEC-017).

Mã unique: dữ liệu cấp tenant theo `(tenant_id, code)`; dữ liệu cấp công ty theo `(tenant_id, company_id, code)`.

### 3. Công ty hiện hành: lấy từ token, không nhận từ client

Luồng đăng nhập:

1. Người dùng nhập username + mật khẩu. Username unique **toàn nền tảng** (vì không nhập mã công ty), từ đó xác định tenant.
2. Server trả danh sách công ty user được vào (`UserCompanyAccess`). Có đúng một công ty thì vào thẳng; không có công ty nào thì không cho vào và báo liên hệ quản trị.
3. User chọn công ty → server kiểm tra quyền → cấp access token có claim `tenant_id` + `company_id`.
4. Đổi công ty (header) = gọi API chọn công ty → server kiểm tra lại → cấp token mới. FE xóa toàn bộ cache TanStack Query và tải lại trang hiện tại.

`company_id` **không bao giờ** nhận từ body, query string hay header do client gửi, giống `tenant_id`.

### 4. Chế độ xem gộp nhiều công ty (DEC-011)

- Người có quyền `admin.company.view-all` (mặc định gán cho vai trò Ban lãnh đạo) thấy thêm lựa chọn **"Tất cả công ty"** khi chọn công ty. Token khi đó mang `company_mode = all` thay cho `company_id`.
- Danh sách công ty được **server đọc** từ `UserCompanyAccess` của user cho mỗi request (cache ngắn), không đặt danh sách ID vào token.
- Chế độ xem gộp **chỉ đọc**: interceptor chặn mọi insert/update/delete trên `ICompanyEntity` khi không có một công ty hiện hành cụ thể. Muốn tạo hoặc duyệt chứng từ thì chọn lại một công ty.
- Danh sách và báo cáo ở chế độ này luôn có cột/bộ lọc **Công ty**.

### 5. Cơ chế kỹ thuật

| Thành phần | Thay đổi |
|---|---|
| `ICompanyContext` (mới, BuildingBlocks.Contracts) | `Guid? ActiveCompanyId` (null ở chế độ xem gộp) · `IReadOnlyCollection<Guid> ReadableCompanyIds` |
| `TenantDbContextBase` | Thêm filter có tên `Company`: `ReadableCompanyIds.Contains(entity.CompanyId)`. Không có công ty → danh sách rỗng → không trả bản ghi (fail-closed, giống filter Tenant). Thêm index `(tenant_id, company_id)` |
| `CompanyEntityInterceptor` (mới) | Insert: gán `CompanyId` từ `ActiveCompanyId`; không có thì chặn. Update/Delete: bản ghi phải thuộc `ActiveCompanyId`; cấm đổi `CompanyId` |
| `QueryFilterNames` | Thêm `Company`. Tắt filter này chỉ cho tác vụ hệ thống, có comment, giống quy định về `IgnoreQueryFilters()` |
| Architecture test | Entity nghiệp vụ phải implement đúng một trong hai: `ITenantEntity` (đã đăng ký trong danh sách cấp tenant) hoặc `ICompanyEntity`. Entity mới không khai báo cấp → test fail |
| Integration test | Mỗi chức năng cấp công ty có test với 2 công ty cùng tenant: không đọc/sửa chéo; chế độ xem gộp đọc được cả hai nhưng không ghi được |
| Phân quyền | Vai trò gán **theo công ty** (`UserRole` có `company_id`). Quyền hiệu lực = các vai trò của user tại công ty hiện hành. Ở chế độ xem gộp: chỉ áp quyền `view`/`export` có ở ít nhất một công ty, và chỉ đọc công ty mà user có quyền `view` tương ứng |
| Dữ liệu cấp tenant | Sửa danh mục dùng chung ảnh hưởng mọi công ty → chỉ người có quyền `admin.catalog.*` (mặc định vai trò Quản trị) |

### 6. Phạm vi dữ liệu (data scope) bên trong công ty

Tầng công ty lọc trước; data scope thu hẹp tiếp bên trong công ty đó. Các mức: `all` (toàn công ty) · `department` · `department-tree` · `project` · `own`. Mức `company` không còn cần.

Bản ghi thuộc phạm vi khi khớp **một** trong các thuộc tính phạm vi có trên bản ghi (DEC-010). Ví dụ đề nghị thanh toán có cả dự án và phòng ban: người có scope `project = {A}` thấy bản ghi của dự án A dù phòng ban khác, và người có scope `department = {Kế toán}` thấy bản ghi của phòng Kế toán dù dự án khác.

### 7. Nhân viên và người dùng

- Employee là dữ liệu cấp công ty; một nhân viên thuộc đúng một công ty tại một thời điểm (DEC-009).
- User là cấp tenant, liên kết tối đa một Employee. Một user được vào nhiều công ty (vd. kế toán trưởng tập đoàn) mà không cần là nhân viên của từng công ty.
- **Không** điều chuyển nhân viên giữa các công ty (DEC-016). `CompanyId` của nhân viên không bao giờ đổi.

## Hệ quả

- Rule `.claude/rules/multi-tenancy-and-security.md` được cập nhật theo ADR này.
- Mọi đặc tả chức năng phải ghi rõ entity thuộc **cấp tenant** hay **cấp công ty**.
- Báo cáo hợp nhất (cộng gộp nhiều công ty) chỉ là xem gộp, **không** loại trừ giao dịch nội bộ giữa các công ty. Hợp nhất báo cáo tài chính theo chuẩn kế toán nằm ngoài phạm vi.
- Giao dịch giữa các công ty trong tập đoàn (vd. công ty mẹ xuất vật tư cho công ty con): mỗi bên lập chứng từ của mình; chưa có cơ chế tự sinh chứng từ đối ứng.
- Chi phí: thêm một filter cho mỗi truy vấn và một cột `company_id` ở hầu hết bảng nghiệp vụ. Làm từ đầu, trước khi có bảng nghiệp vụ nào, nên không phải migration dữ liệu.

## Phương án đã cân nhắc

- **Mỗi công ty con là một tenant riêng**: tận dụng nguyên cơ chế hiện có, nhưng không dùng chung danh mục, user phải có nhiều tài khoản, ban lãnh đạo không xem gộp được.
- **Công ty chỉ là một thuộc tính lọc thông thường (không có global filter)**: linh hoạt, nhưng mỗi truy vấn tự lọc tay, quên một chỗ là lộ dữ liệu giữa các công ty.
- **Đặt danh sách công ty được phép vào token**: bớt một lần đọc DB mỗi request, nhưng token phình to và gỡ quyền một công ty phải chờ token hết hạn.
