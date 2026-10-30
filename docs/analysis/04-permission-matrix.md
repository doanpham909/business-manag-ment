# 04 — Ma trận phân quyền sơ bộ

## 1. Nguyên tắc (theo `multi-tenancy-and-security.md`)

- **[C]** Mã quyền `{module}.{resource}.{action}`, khai báo tập trung ở backend, FE có bản tương ứng để ẩn/hiện.
- **[C]** Backend kiểm tra quyền ở mọi endpoint. Ẩn nút ở FE chỉ là trải nghiệm.
- **[C]** Role do từng tenant tự định nghĩa; User ↔ Role nhiều–nhiều; quyền hiệu lực = hợp các role.
- **[C]** Lọc data scope ở tầng query, áp như nhau cho danh sách, chi tiết, export, báo cáo.
- **[C]** Platform admin không xem dữ liệu nghiệp vụ của tenant.

## 2. Action

| Action | Trạng thái | Dùng cho |
|---|---|---|
| `view`, `create`, `update`, `delete`, `approve`, `export` | **[C]** chuẩn của repo | |
| `import` | **[C]** DEC-015 | Import Excel ghi hàng loạt; tách khỏi `create` vì rủi ro cao hơn |
| `submit` | **[C]** DEC-015 | Gửi duyệt. Người tạo nháp chưa chắc được gửi (vd. nhân viên nhập bảng công, trưởng phòng gửi) |
| `lock` | **[C]** DEC-015 | Khóa/mở khóa kỳ: bảng công, bảng lương, kỳ kế toán, kiểm kê |
| `view-sensitive` | **[C]** DEC-015 | Xem trường nhạy cảm: định danh, số tài khoản, lương |
| Reject | **[C]** gộp vào `approve` | Người được duyệt thì cũng được từ chối ở cùng bước |
| Manage | **[C]** bỏ | Mơ hồ; thay bằng tổ hợp quyền cụ thể |

Quyền duyệt theo bước nằm ở **cấu hình workflow** (ADM-06), không chỉ ở mã `approve`. `approve` là điều kiện cần; người duyệt phải đúng người/vai trò của bước hiện tại.

## 3. Phạm vi dữ liệu (data scope)

Repo hiện có `own`, `department`, `all`. Master prompt cần 7 mức. **[C]** DEC-015:

| Mã | Nghĩa | Thay cho |
|---|---|---|
| `all` | Toàn bộ **công ty đang chọn** | Toàn doanh nghiệp / công ty |
| `department` | Đúng phòng ban của người dùng | |
| `department-tree` | Phòng ban + đơn vị cấp dưới | |
| `project` | Các dự án được gán (danh sách ID) | |
| `own` | Bản ghi do mình tạo **hoặc** mình là người phụ trách/được phân công | Gộp "cá nhân" và "đối tượng được phân công" |

- **[C]** Tầng công ty (DEC-003, DEC-005) đứng trên scope: mọi truy vấn trước hết lọc theo công ty đang chọn trong token; scope chỉ thu hẹp tiếp trong công ty đó. Vì thế bỏ mức `company` riêng. Xem gộp nhiều công ty: xem dòng DEC-011 bên dưới.
- Vai trò và scope gán **theo từng công ty** **[C]** (ADR-0002).
- Scope gắn với role theo **từng module** (một người có thể `all` ở kho nhưng `project` ở kế toán) **[C]** (DEC-015).
- Nhiều role → lấy hợp các phạm vi **[C]** (DEC-015).
- **[C]** Bản ghi thuộc phạm vi khi **một** trong các thuộc tính (phòng ban, dự án, người tạo/phụ trách) rơi vào phạm vi (DEC-010).
- **[C]** Chế độ "Tất cả công ty": quyền `admin.company.view-all`, mặc định gán R02 Ban lãnh đạo; chỉ đọc (DEC-011).
- **[C]** Tạo công ty: `admin.company.create`, mặc định R01 Quản trị (DEC-012).
- Phạm vi "phòng ban" của người dùng lấy từ `EmployeeAssignment` hiện hành của nhân viên liên kết. User chưa liên kết nhân viên → scope `department*` cho kết quả rỗng **[C]** (DEC-015).

## 4. Vai trò mẫu (seed khi tạo tenant)

**[P]** Tenant được sửa/xóa/thêm. Tên vai trò cần PO xác nhận → `Q-ADM-01`.

| Mã | Vai trò | Scope mặc định |
|---|---|---|
| R01 | Quản trị hệ thống (tenant admin) | `all` (chỉ `admin.*`, không mặc định xem lương) |
| R02 | Ban lãnh đạo | `all`, chỉ xem + duyệt |
| R03 | Trưởng phòng / đơn vị | `department-tree` |
| R04 | Chuyên viên nhân sự | `all` trong `hrm` |
| R05 | Chuyên viên tiền lương | `all` trong `payroll` |
| R06 | Kế toán viên | `all` hoặc `project` trong `accounting` |
| R07 | Kế toán trưởng | `all` trong `accounting` |
| R08 | Nhân viên mua hàng | `project` trong `inventory` |
| R09 | Thủ kho | Kho được gán (qua `project` hoặc gán kho trực tiếp **[U]** `Q-WH-04`) |
| R10 | Ban điều hành dự án / Chỉ huy trưởng | `project` |
| R11 | Nhân viên (mặc định mọi user) | `own` |

## 5. Ma trận sơ bộ

Ký hiệu: **V** view · **C** create · **U** update · **D** delete · **S** submit · **A** approve/reject · **I** import · **E** export · **L** lock · **N** view-sensitive · `—` không có. Ô trống = không có quyền.

| Resource (mã) | R01 Admin | R02 Lãnh đạo | R03 Trưởng phòng | R04 NS | R05 Lương | R06 KT | R07 KTT | R08 Mua hàng | R09 Thủ kho | R10 BĐH dự án | R11 NV |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `admin.user` | VCUDE | V |  |  |  |  |  |  |  |  |  |
| `admin.role`, `admin.datascope` | VCUD |  |  |  |  |  |  |  |  |  |  |
| `admin.catalog` | VCUDIE | V | V | V | V | VCU | VCUD | V | V | V | V |
| `admin.workflow`, `admin.setting` | VCU |  |  |  |  |  |  |  |  |  |  |
| `admin.auditlog` | VE | V |  |  |  |  | V |  |  |  |  |
| `hrm.department` | V | V | V | VCUDIE | V | V | V | V | V | V | V |
| `hrm.position` | V | V | V | VCUDIE | V |  |  |  |  |  |  |
| `hrm.employee` | V | VE | V | VCUDIEN | VN | V | V |  |  | V | own: V |
| `payroll.schedule` |  | V | VCU | V | VCUDI |  |  |  |  | VCU |  |
| `payroll.attendance` |  | V | VCUS + A (điều chỉnh) | V | VCUDIE |  |  |  |  | VCUS | own: V, C (đề nghị điều chỉnh) |
| `payroll.timesheet` |  | VA | VUSA | V | VCUSEL |  |  |  |  | VUS | own: V |
| `payroll.payroll` |  | VAN | V (đơn vị, không N) |  | VCUSELN | V | VA N |  |  |  |  |
| `payroll.payslip` |  |  |  |  | VCE |  |  |  |  |  | own: V |
| `payroll.settlement` |  | VA |  |  | VCUSE | V | VAL |  |  |  |  |
| `accounting.budget` |  | VA | VCS (đơn vị) |  |  | VCUSIE | VAL |  |  | VCS |  |
| `accounting.cost-*` |  | V | V |  |  | VCUIE | VCUDA |  |  | V |  |
| `accounting.payment-request` |  | VA | VCUSA | VCUS | VCUS | VCUSE | VAE | VCUS | VCUS | VCUSA | own: VCUS |
| `accounting.payment` (chi tiền) |  | V |  |  |  | VCU | VCUA |  |  |  |  |
| `accounting.advance` |  | VA | VCUSA |  |  | VCUSE | VAE |  |  | VCUSA | own: VCUS |
| `accounting.debt`, `accounting.revenue`, `accounting.cashflow` |  | VE |  |  |  | VCUE | VCUAE |  |  |  |  |
| `accounting.journal` |  | V |  |  |  | VCUIE | VCUDAEL |  |  |  |  |
| `accounting.report` |  | VE | V (đơn vị) |  |  | VE | VE |  |  | VE |  |
| `inventory.material`, `inventory.supplier` | V | V | V |  |  | V | V | VCUIE | VCU | V |  |
| `inventory.warehouse` | V | V |  |  |  | V | V | V | VU | VCU |  |
| `inventory.material-request` |  | V | VCUSA |  |  |  |  | V | V | VCUSA | own: VCUS |
| `inventory.purchase` |  | VA | V |  |  | V | VA | VCUSE |  | VCUSA |  |
| `inventory.receipt`, `inventory.issue` |  | V |  |  |  | V | V | V | VCUA(xác nhận)E | V |  |
| `inventory.transfer` |  | V |  |  |  |  |  | V | VCUSA | VA |  |
| `inventory.stock` |  | VE | V |  |  | VE | VE | VE | VE | VE |  |
| `inventory.stocktake` |  | VA |  |  |  | V | VA | V | VCUIS | VA |  |
| `projects.project` | V | VE | V |  |  | V | V | V | V | VCUIE |  |

**[A]** Ma trận trên là điểm khởi đầu để PO sửa, không phải chính sách phân quyền. Các ô có `A` thực tế còn phụ thuộc cấu hình bước duyệt.

## 6. Quy tắc phân quyền bắt buộc

| Mã | Quy tắc | |
|---|---|---|
| BR-SEC-01 | Không gỡ được vai trò quản trị khỏi người quản trị cuối cùng của tenant; không khóa tài khoản đó | **[C]** |
| BR-SEC-02 | Người dùng không tự gán cho mình quyền hoặc scope rộng hơn quyền đang có | **[C]** |
| BR-SEC-03 | Người tạo chứng từ không tự duyệt chứng từ của mình **[A]** | `Q-ADM-08` |
| BR-SEC-04 | Trường nhạy cảm (định danh, tài khoản ngân hàng, lương) chỉ trả về khi có `view-sensitive`; export cũng tuân theo | **[C]** (DEC-015) |
| BR-SEC-05 | Thay đổi role, permission, scope, gán user ↔ role đều ghi audit log | **[C]** |
| BR-SEC-06 | Đổi đơn vị/dự án của nhân viên → quyền hiệu lực tính lại ngay ở lần gọi API tiếp theo (không cache quá 1 phút) | **[P]** |
| BR-SEC-07 | Tài khoản bị khóa: không đăng nhập mới; phiên đang mở bị thu hồi refresh token | **[P]** |
