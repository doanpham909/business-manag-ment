# 02 — Sitemap, Menu Sidebar, Danh sách màn hình

Route theo quy ước repo: slug tiếng Việt, khai báo tập trung ở `ROUTE_PATHS` + `features/{module}/{module}-routes.tsx`. Route gốc của 4 module có sẵn giữ nguyên **[C]**; route con và 2 module mới là **[P]**.

## 1. Sitemap

```text
/                                   Trang chủ (dashboard tổng quan theo quyền)
/dang-nhap                          Đăng nhập
/chon-cong-ty                       Chọn công ty làm việc
/tai-khoan                          Hồ sơ cá nhân · đổi mật khẩu · phiếu lương của tôi
/thong-bao                          Trung tâm thông báo
/viec-cho-duyet                     Hộp việc chờ tôi duyệt (gom từ mọi phân hệ)

/quan-tri                           ADM
  /nguoi-dung                       ADM-01
  /vai-tro                          ADM-02
  /pham-vi-truy-cap                 ADM-03
  /cau-hinh                         ADM-04
  /danh-muc/:loai                   ADM-05 (một màn hình chung, :loai = don-vi-tinh, loai-vat-tu, …)
  /quy-trinh                        ADM-06
  /thong-bao                        ADM-07 (cấu hình loại thông báo)
  /nhat-ky                          ADM-08

/nhan-su                            HRM
  /co-cau-to-chuc                   HRM-01
  /nhan-vien                        HRM-02
  /chuc-danh                        HRM-03

/cham-cong-luong                    PAY
  /lich-lam-viec                    PAY-01
  /cham-cong                        PAY-02
  /bang-cong                        PAY-03
  /bang-luong                       PAY-04
  /phieu-luong                      PAY-05
  /quyet-toan                       PAY-06

/ke-toan                            FIN
  /ngan-sach                        FIN-01
  /chi-phi/vat-tu                   FIN-02
  /chi-phi/nhan-cong                FIN-03
  /chi-phi/may                      FIN-04
  /chi-phi/chung                    FIN-05
  /de-nghi-thanh-toan               FIN-06
  /cong-no                          FIN-07
  /tam-ung                          FIN-08
  /doanh-thu                        FIN-09
  /dong-tien                        FIN-10
  /hach-toan                        FIN-11
  /bao-cao/:ma                      FIN-12

/kho                                WH
  /tong-quan                        WH-08 dashboard
  /vat-tu                           WH-01
  /danh-sach-kho                    WH-02
  /yeu-cau-vat-tu                   WH-03
  /mua-hang                         WH-04
  /nhap-kho                         WH-05
  /xuat-kho                         WH-06
  /chuyen-kho                       WH-07
  /ton-kho                          WH-08
  /kiem-ke                          WH-09

/du-an                              PRJ
  /danh-sach                        PRJ-01
```

Quy ước con của mỗi chức năng **[P]**: `/{chuc-nang}` (danh sách) · `/{chuc-nang}/tao-moi` · `/{chuc-nang}/:id` (chi tiết) · `/{chuc-nang}/:id/sua`. Form ≤ 6 trường mở bằng Drawer trên trang danh sách thay vì route riêng.

## 2. Menu Sidebar

Menu sinh tự động từ `appRoutes` **[C]**. Mục chỉ hiện khi người dùng có quyền `view` tương ứng **[C]**. Thứ tự nhóm theo master prompt.

| Nhóm (icon `lucide-react` [P]) | Mục menu | Quyền cần để hiện |
|---|---|---|
| **Admin** (`Settings`) | Người dùng | `admin.user.view` |
| | Vai trò & phân quyền | `admin.role.view` |
| | Phạm vi truy cập | `admin.datascope.view` |
| | Danh mục dùng chung | `admin.catalog.view` |
| | Quy trình phê duyệt | `admin.workflow.view` |
| | Thông báo | `admin.notification.view` |
| | Nhật ký hệ thống | `admin.auditlog.view` |
| | Cấu hình hệ thống | `admin.setting.view` |
| **Tổ chức – Nhân sự** (`Users`) | Cơ cấu tổ chức | `hrm.department.view` |
| | Hồ sơ nhân viên | `hrm.employee.view` |
| | Vị trí & chức danh | `hrm.position.view` |
| **Chấm công – Tính lương** (`CalendarClock`) | Lịch làm việc | `payroll.schedule.view` |
| | Chấm công | `payroll.attendance.view` |
| | Bảng công | `payroll.timesheet.view` |
| | Bảng lương | `payroll.payroll.view` |
| | Phiếu lương | `payroll.payslip.view` |
| | Quyết toán lương | `payroll.settlement.view` |
| **Tài chính – Kế toán** (`Landmark`) | Ngân sách | `accounting.budget.view` |
| | Chi phí ▸ Vật tư · Nhân công · Máy móc · Chi phí chung | `accounting.cost-*.view` |
| | Đề nghị thanh toán | `accounting.payment-request.view` |
| | Tạm ứng | `accounting.advance.view` |
| | Công nợ | `accounting.debt.view` |
| | Doanh thu | `accounting.revenue.view` |
| | Dòng tiền | `accounting.cashflow.view` |
| | Hạch toán ▸ Hệ thống tài khoản · Chứng từ · Sổ nhật ký · Sổ cái · Khóa kỳ | `accounting.journal.view` |
| | Báo cáo | `accounting.report.view` |
| **Quản lý kho** (`Warehouse`) | Tổng quan kho | `inventory.stock.view` |
| | Danh mục vật tư | `inventory.material.view` |
| | Danh sách kho | `inventory.warehouse.view` |
| | Yêu cầu vật tư | `inventory.material-request.view` |
| | Mua hàng ▸ Yêu cầu mua · Đơn mua · Nhà cung cấp | `inventory.purchase.view` |
| | Nhập kho · Xuất kho · Chuyển kho | `inventory.receipt/issue/transfer.view` |
| | Tồn kho & thẻ kho | `inventory.stock.view` |
| | Kiểm kê | `inventory.stocktake.view` |
| **Quản lý dự án** (`Building2`) | Danh mục dự án | `projects.project.view` |

Hành vi **[C]**: nhiều cấp, expand/collapse, active state, thu gọn sidebar, ghi nhớ trạng thái (đã có `use-layout-store`).
**[U]** Nhà cung cấp thuộc WH hay ADM-05? Đề xuất đặt ở WH vì là dữ liệu chủ của mua hàng; FIN-07 tham chiếu → `Q-WH-05`.

## 3. Header

| Thành phần | Ghi chú |
|---|---|
| Logo + tên doanh nghiệp | Lấy từ ADM-04 |
| Breadcrumb | Đã có (`app-breadcrumb.tsx`) |
| Chọn công ty | **[C]** Luôn hiện công ty đang chọn; đổi được nếu user có quyền ở ≥ 2 công ty (DEC-005) |
| Chọn dự án | Chỉ ở màn hình hỗ trợ lọc theo dự án; là bộ lọc URL, không đổi phạm vi quyền **[P]** |
| Chuông thông báo | ADM-07 |
| Avatar + menu tài khoản | Hồ sơ, đổi mật khẩu, đăng xuất |

## 4. Danh sách màn hình

Loại: **L** List · **D** Detail · **F** Form (trang) · **Dr** Drawer · **M** Modal · **G** Grid kiểu Excel · **Db** Dashboard · **T** Tree/Chart.

### Chung

| Screen ID | Tên | Loại |
|---|---|---|
| CMN-S01 | Đăng nhập | F |
| CMN-S01b | Chọn công ty (sau đăng nhập) | F |
| CMN-S02 | Đổi mật khẩu / quên mật khẩu | F |
| CMN-S03 | Trang chủ (dashboard theo vai trò) | Db |
| CMN-S04 | Trung tâm thông báo | L |
| CMN-S05 | Hộp việc chờ duyệt | L |
| CMN-S06 | Import Wizard (dùng chung) | M |
| CMN-S07 | Lịch sử duyệt / Audit của một bản ghi (dùng chung) | Dr |
| CMN-S08 | 403 · 404 · lỗi | — |

### ADM

| Screen ID | Tên | Loại |
|---|---|---|
| ADM-00-01 | Danh sách công ty (pháp nhân) | L |
| ADM-00-02 | Tạo / sửa công ty | Dr |
| ADM-01-01 | Danh sách người dùng | L |
| ADM-01-02 | Tạo / cập nhật người dùng | Dr |
| ADM-01-03 | Chi tiết tài khoản (tab: thông tin · vai trò · quyền hiệu lực · lịch sử đăng nhập · audit) | D |
| ADM-01-04 | Gán vai trò và phạm vi | M |
| ADM-02-01 | Danh sách vai trò | L |
| ADM-02-02 | Tạo / sửa vai trò | Dr |
| ADM-02-03 | Ma trận phân quyền | G |
| ADM-02-04 | Quyền hiệu lực (theo user hoặc theo vai trò) | D |
| ADM-03-01 | Cấu hình phạm vi truy cập | L |
| ADM-03-02 | Gán phạm vi cho user / vai trò | M |
| ADM-03-03 | Xem trước dữ liệu được phép truy cập | D |
| ADM-04-01 | Cấu hình hệ thống (tab theo nhóm) | F |
| ADM-05-01 | Danh mục dùng chung — danh sách theo loại | L |
| ADM-05-02 | Tạo / sửa mục danh mục | Dr |
| ADM-06-01 | Danh sách quy trình | L |
| ADM-06-02 | Thiết kế quy trình (bước · người duyệt · điều kiện · xem trước luồng) | F |
| ADM-06-03 | Lịch sử phiên bản quy trình | Dr |
| ADM-07-01 | Cấu hình loại thông báo | L |
| ADM-08-01 | Nhật ký thay đổi dữ liệu | L |
| ADM-08-02 | Chi tiết thay đổi (trước/sau) | Dr |
| ADM-08-03 | Lịch sử đăng nhập | L |

### HRM

| Screen ID | Tên | Loại |
|---|---|---|
| HRM-01-01 | Cây tổ chức + panel chi tiết đơn vị | T |
| HRM-01-02 | Sơ đồ tổ chức (zoom, pan) | T |
| HRM-01-03 | Danh sách đơn vị | L |
| HRM-01-04 | Tạo / sửa đơn vị | Dr |
| HRM-01-05 | Chi tiết đơn vị (nhân sự trực thuộc, đơn vị con) | D |
| HRM-02-01 | Danh sách nhân viên | L |
| HRM-02-02 | Tạo / cập nhật hồ sơ (5 tab) | F |
| HRM-02-03 | Chi tiết nhân viên | D |
| HRM-02-04 | Import nhân viên | M (CMN-S06) |
| HRM-02-05 | Lịch sử thay đổi hồ sơ | Dr |
| HRM-03-01 | Danh sách vị trí / chức danh | L |
| HRM-03-02 | Tạo / sửa | Dr |
| HRM-03-03 | Chi tiết + nhân viên thuộc vị trí | D |

### PAY

| Screen ID | Tên | Loại |
|---|---|---|
| PAY-01-01 | Danh sách ca làm việc | L |
| PAY-01-02 | Tạo / sửa ca | Dr |
| PAY-01-03 | Lịch làm việc tháng (calendar view) | T |
| PAY-01-04 | Phân lịch theo phòng ban / nhân viên (bảng, sửa hàng loạt, sao chép kỳ) | G |
| PAY-02-01 | Dữ liệu chấm công (ngày/tháng) | L |
| PAY-02-02 | Nhập / điều chỉnh chấm công | Dr |
| PAY-02-03 | Import chấm công | M |
| PAY-02-04 | Danh sách yêu cầu điều chỉnh + duyệt | L |
| PAY-03-01 | Danh sách kỳ bảng công | L |
| PAY-03-02 | Bảng công tháng (cột 01–31 + tổng) | G |
| PAY-03-03 | Cảnh báo dữ liệu bất thường | Dr |
| PAY-04-01 | Danh sách kỳ lương | L |
| PAY-04-02 | Bảng lương tháng | G |
| PAY-04-03 | Chi tiết lương một nhân viên (diễn giải công thức) | Dr |
| PAY-04-04 | Thành phần lương & tham số | L |
| PAY-04-05 | Lịch sử các lần tính | Dr |
| PAY-05-01 | Danh sách phiếu lương | L |
| PAY-05-02 | Xem phiếu lương (+ PDF) | D |
| PAY-05-03 | Phiếu lương của tôi | D |
| PAY-06-01 | Danh sách kỳ quyết toán | L |
| PAY-06-02 | Bảng đối chiếu lương phải trả / đã trả | G |
| PAY-06-03 | Chi tiết quyết toán + điều chỉnh | D |

### FIN

| Screen ID | Tên | Loại |
|---|---|---|
| FIN-01-01 | Danh sách ngân sách | L |
| FIN-01-02 | Lập ngân sách (khoản mục × kỳ) | G |
| FIN-01-03 | Chi tiết ngân sách + phiên bản điều chỉnh | D |
| FIN-01-04 | So sánh kế hoạch / thực tế | Db |
| FIN-02-01 | Tổng hợp chi phí vật tư (theo dự án / vật tư) | L |
| FIN-02-02 | Chi tiết + chứng từ nguồn | Dr |
| FIN-03-01 | Tổng hợp chi phí nhân công | L |
| FIN-03-02 | Chi tiết theo kỳ lương | Dr |
| FIN-04-01 | Danh sách chi phí máy | L |
| FIN-04-02 | Ghi nhận chi phí máy | Dr |
| FIN-05-01 | Danh sách chi phí chung | L |
| FIN-05-02 | Khai báo chi phí + phân bổ | F |
| FIN-06-01 | Danh sách đề nghị thanh toán | L |
| FIN-06-02 | Tạo / sửa đề nghị | F |
| FIN-06-03 | Chi tiết đề nghị (timeline duyệt, lần chi) | D |
| FIN-06-04 | Ghi nhận chi tiền (từng phần) | M |
| FIN-07-01 | Dashboard công nợ | Db |
| FIN-07-02 | Phải thu / phải trả theo đối tượng | L |
| FIN-07-03 | Chi tiết công nợ một đối tượng + đối chiếu | D |
| FIN-07-04 | Báo cáo tuổi nợ | L |
| FIN-08-01 | Danh sách tạm ứng | L |
| FIN-08-02 | Tạo đề nghị tạm ứng | Dr |
| FIN-08-03 | Chi tiết + hoàn ứng / quyết toán tạm ứng | D |
| FIN-09-01 | Tổng hợp doanh thu | L |
| FIN-09-02 | Ghi nhận / điều chỉnh doanh thu | Dr |
| FIN-10-01 | Dashboard dòng tiền | Db |
| FIN-10-02 | Kế hoạch dòng tiền | G |
| FIN-11-01 | Hệ thống tài khoản | T |
| FIN-11-02 | Danh sách chứng từ kế toán | L |
| FIN-11-03 | Tạo / sửa chứng từ + bút toán | F |
| FIN-11-04 | Sổ nhật ký chung | L |
| FIN-11-05 | Sổ cái | L |
| FIN-11-06 | Khóa kỳ kế toán | L |
| FIN-12-01 | Danh mục báo cáo | L |
| FIN-12-02 | Trình xem báo cáo (lọc, bảng nhiều cấp, biểu đồ, drill-down, xuất) | Db |

### WH

| Screen ID | Tên | Loại |
|---|---|---|
| WH-01-01 | Danh sách vật tư | L |
| WH-01-02 | Tạo / sửa vật tư (+ đơn vị quy đổi) | Dr |
| WH-01-03 | Chi tiết vật tư (tồn theo kho, giao dịch gần nhất) | D |
| WH-02-01 | Danh sách kho | L |
| WH-02-02 | Tạo / sửa kho | Dr |
| WH-02-03 | Chi tiết kho | D |
| WH-03-01 | Danh sách yêu cầu vật tư | L |
| WH-03-02 | Tạo / sửa yêu cầu (nhiều dòng) | F |
| WH-03-03 | Chi tiết + theo dõi cấp phát | D |
| WH-04-01 | Danh sách yêu cầu mua | L |
| WH-04-02 | Tạo yêu cầu mua | F |
| WH-04-03 | Danh sách đơn mua | L |
| WH-04-04 | Tạo / sửa đơn mua | F |
| WH-04-05 | Chi tiết đơn mua + theo dõi nhận hàng | D |
| WH-04-06 | Danh sách / form nhà cung cấp | L + Dr |
| WH-05-01 | Danh sách phiếu nhập | L |
| WH-05-02 | Tạo phiếu nhập (từ đơn mua / thủ công) | F |
| WH-05-03 | Chi tiết + xác nhận nhập | D |
| WH-06-01 | Danh sách phiếu xuất | L |
| WH-06-02 | Tạo phiếu xuất (từ yêu cầu / thủ công) | F |
| WH-06-03 | Chi tiết + xác nhận xuất | D |
| WH-07-01 | Danh sách phiếu chuyển | L |
| WH-07-02 | Tạo phiếu chuyển | F |
| WH-07-03 | Chi tiết + xác nhận kho nhận + xử lý chênh lệch | D |
| WH-08-01 | Dashboard tồn kho | Db |
| WH-08-02 | Tồn kho tổng hợp / theo kho / theo vật tư | L |
| WH-08-03 | Thẻ kho | L |
| WH-09-01 | Danh sách đợt kiểm kê | L |
| WH-09-02 | Bảng kiểm kê (nhập số thực kiểm) | G |
| WH-09-03 | Đối chiếu chênh lệch + duyệt điều chỉnh | D |

### PRJ

| Screen ID | Tên | Loại |
|---|---|---|
| PRJ-01-01 | Danh sách dự án | L |
| PRJ-01-02 | Tạo dự án | F |
| PRJ-01-03 | Cập nhật dự án | F |
| PRJ-01-04 | Chi tiết dự án (tab: thông tin · dữ liệu liên kết chỉ đọc: kho, ngân sách, chi phí, đề nghị thanh toán) | D |

### Tổng số màn hình

| CMN | ADM | HRM | PAY | FIN | WH | PRJ | Cộng |
|---|---|---|---|---|---|---|---|
| 9 | 23 | 13 | 22 | 35 | 30 | 4 | **136** |
