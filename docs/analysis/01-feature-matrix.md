# 01 — Feature Matrix

Ưu tiên **[P]**: **P0** = nền tảng, phải có trước khi làm phân hệ khác · **P1** = bản prototype đầu tiên của phân hệ · **P2** = sau khi P1 được duyệt.
Phạm vi tính năng là **[C]** (lấy từ master prompt). Ưu tiên và phụ thuộc là **[P]**.

Ký hiệu cột chức năng: L = danh sách · F = form tạo/sửa · D = chi tiết · I/E = import/export · W = có quy trình duyệt · K = khóa/chốt kỳ.

## ADM — Admin (`admin`)

| ID | Chức năng | L | F | D | I/E | W | K | Ưu tiên | Phụ thuộc | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|---|
| ADM-00 | Quản lý công ty (pháp nhân) | ✓ | ✓ | ✓ |  |  |  | P0 | — | Thêm theo DEC-003 |
| ADM-01 | Quản lý người dùng | ✓ | ✓ | ✓ | E |  |  | P0 | ADM-00, ADM-02, HRM-02 (liên kết NV) | Gồm khóa/mở, đặt lại mật khẩu, xem quyền hiệu lực, gán công ty được truy cập |
| ADM-02 | Vai trò và phân quyền | ✓ | ✓ | ✓ |  |  |  | P0 | — | Ma trận Module → Feature → Action |
| ADM-03 | Cơ cấu truy cập (data scope) | ✓ | ✓ | ✓ |  |  |  | P0 | ADM-02, HRM-01, PRJ-01 | Xem trước dữ liệu được phép truy cập |
| ADM-04 | Cấu hình hệ thống | — | ✓ | — |  |  |  | P1 | — | Định dạng, tiền tệ, múi giờ, upload, phiên |
| ADM-05 | Danh mục dùng chung | ✓ | ✓ | ✓ | I/E |  |  | P0 | — | ĐVT, loại vật tư, nhóm chi phí, loại dự án… |
| ADM-06 | Workflow phê duyệt | ✓ | ✓ | ✓ |  |  |  | P1 | ADM-02, HRM-01 | Engine dùng chung cho mọi chứng từ có W |
| ADM-07 | Thông báo | ✓ | ✓ | ✓ |  |  |  | P1 | ADM-06 | Notification center + cấu hình loại |
| ADM-08 | Nhật ký hệ thống | ✓ | — | ✓ | E |  |  | P0 (ghi) / P1 (màn hình) | — | Ghi audit từ đầu; màn hình xem có thể làm sau |

## HRM — Tổ chức – Nhân sự (`hrm`)

| ID | Chức năng | L | F | D | I/E | W | K | Ưu tiên | Phụ thuộc | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|---|
| HRM-01 | Cơ cấu tổ chức | ✓ | ✓ | ✓ | I/E |  |  | P0 | ADM-05 | Tree view + sơ đồ tổ chức |
| HRM-02 | Hồ sơ nhân viên | ✓ | ✓ | ✓ | I/E |  |  | P1 | HRM-01, HRM-03 | 5 tab; dữ liệu nhạy cảm có quyền riêng |
| HRM-03 | Vị trí và chức danh | ✓ | ✓ | ✓ | I/E |  |  | P1 | HRM-01 | Phân biệt vị trí vs chức danh → `Q-HRM-01` |

## PAY — Chấm công – Tính lương (`payroll`)

| ID | Chức năng | L | F | D | I/E | W | K | Ưu tiên | Phụ thuộc | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|---|
| PAY-01 | Lịch làm việc / ca | ✓ | ✓ | ✓ |  |  |  | P1 | HRM-01, HRM-02 | Calendar view + table view, ca qua ngày |
| PAY-02 | Chấm công (dữ liệu vào/ra) | ✓ | ✓ | ✓ | I/E | ✓ (điều chỉnh) |  | P1 | PAY-01 | Nguồn: nhập tay, Excel; máy chấm công → `Q-PAY-01` |
| PAY-03 | Bảng công tháng | ✓ | ✓ (ô) | ✓ | E | ✓ | ✓ | P1 | PAY-02, PRJ-01 | Lưới 31 ngày dạng Excel |
| PAY-04 | Bảng lương | ✓ | ✓ (ô) | ✓ | E | ✓ | ✓ | P2 | PAY-03, chính sách lương (`Q-PAY-03…07`) | **Bị chặn** cho tới khi có công thức được xác nhận |
| PAY-05 | Phiếu lương | ✓ | — | ✓ | PDF |  | ✓ (phát hành) | P2 | PAY-04 | Nhân viên chỉ xem của mình |
| PAY-06 | Quyết toán lương kỳ | ✓ | ✓ | ✓ | E | ✓ | ✓ | P2 | PAY-04, FIN-06 | Không gồm quyết toán nghỉ việc ([A]) |

## FIN — Tài chính – Kế toán (`accounting`)

| ID | Chức năng | L | F | D | I/E | W | K | Ưu tiên | Phụ thuộc | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|---|
| FIN-01 | Ngân sách | ✓ | ✓ | ✓ | I/E | ✓ | ✓ (phiên bản) | P1 | PRJ-01, ADM-05 | So sánh kế hoạch/thực tế cần FIN-02…05 |
| FIN-02 | Chi phí vật tư | ✓ | — | ✓ | E |  |  | P2 | WH-05, WH-06, `Q-WH-01` | Chủ yếu tổng hợp từ WH |
| FIN-03 | Chi phí nhân công | ✓ | — | ✓ | E |  |  | P2 | PAY-04 | Nhận từ bảng lương đã khóa |
| FIN-04 | Chi phí máy móc | ✓ | ✓ | ✓ | I/E |  |  | P1 | PRJ-01, ADM-05 | Chỉ ghi nhận chi phí, không quản lý tài sản máy |
| FIN-05 | Chi phí chung | ✓ | ✓ | ✓ | I/E |  |  | P1 | PRJ-01, ADM-05 | Phân bổ → `Q-FIN-04` |
| FIN-06 | Đề nghị thanh toán | ✓ | ✓ | ✓ | E | ✓ |  | P1 | ADM-06, PRJ-01 | Tách trạng thái duyệt và trạng thái chi |
| FIN-07 | Công nợ | ✓ | ✓ | ✓ | E |  |  | P2 | FIN-06, WH-04 | Phải thu / phải trả, tuổi nợ |
| FIN-08 | Tạm ứng | ✓ | ✓ | ✓ | E | ✓ |  | P1 | ADM-06, HRM-02 | Hoàn ứng, quyết toán tạm ứng |
| FIN-09 | Doanh thu | ✓ | ✓ | ✓ | E |  |  | P2 | PRJ-01, `Q-FIN-05` | Không suy ra từ sản lượng/tiền thu |
| FIN-10 | Dòng tiền | ✓ | ✓ (kế hoạch) | ✓ | E |  |  | P2 | FIN-06, FIN-08, FIN-09 | Dashboard + kế hoạch thu chi |
| FIN-11 | Hạch toán | ✓ | ✓ | ✓ | I/E |  | ✓ | P2 | Chế độ kế toán `Q-FIN-01` | **Bị chặn** cho tới khi có chính sách kế toán |
| FIN-12 | Báo cáo quản trị | ✓ | — | ✓ | E/PDF |  |  | P2 | FIN-01…11 | 14 báo cáo, có drill-down |

## WH — Quản lý kho (`inventory`)

| ID | Chức năng | L | F | D | I/E | W | K | Ưu tiên | Phụ thuộc | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|---|
| WH-01 | Danh mục vật tư | ✓ | ✓ | ✓ | I/E |  |  | P1 | ADM-05 | Có đơn vị quy đổi |
| WH-02 | Quản lý kho | ✓ | ✓ | ✓ | E |  |  | P1 | PRJ-01, HRM-02 (thủ kho) | Kho có thể gắn dự án |
| WH-03 | Yêu cầu vật tư | ✓ | ✓ | ✓ | I/E | ✓ |  | P1 | WH-01, WH-02, ADM-06 | Cấp phát nhiều lần |
| WH-04 | Mua hàng (yêu cầu mua, đơn mua) | ✓ | ✓ | ✓ | E | ✓ |  | P1 | WH-03, Supplier | Không có hợp đồng mua hàng độc lập |
| WH-05 | Nhập kho | ✓ | ✓ | ✓ | E | (xác nhận) |  | P1 | WH-04 | Chỉ phiếu xác nhận mới tăng tồn |
| WH-06 | Xuất kho | ✓ | ✓ | ✓ | E | (xác nhận) |  | P1 | WH-03, WH-08 | Kiểm tồn khả dụng |
| WH-07 | Chuyển kho | ✓ | ✓ | ✓ | E | ✓ |  | P2 | WH-05, WH-06 | Có trạng thái hàng đang đi đường |
| WH-08 | Tồn kho, thẻ kho | ✓ | — | ✓ | E |  |  | P1 | WH-05, WH-06 | Giá trị tồn → `Q-WH-01` |
| WH-09 | Kiểm kê | ✓ | ✓ | ✓ | I/E | ✓ | ✓ (mốc) | P2 | WH-08 | Không tự điều chỉnh tồn khi chưa duyệt |

## PRJ — Quản lý dự án (`projects`)

| ID | Chức năng | L | F | D | I/E | W | K | Ưu tiên | Phụ thuộc | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|---|
| PRJ-01 | Danh mục dự án | ✓ | ✓ | ✓ | I/E |  |  | P0 | HRM-01, ADM-05 | Chức năng duy nhất của phân hệ. Tab "Dữ liệu liên kết" chỉ đọc |

## Thành phần kỹ thuật dùng chung (không phải phân hệ)

| ID | Thành phần | Ưu tiên | Dùng bởi |
|---|---|---|---|
| CMN-01 | Xác thực (đăng nhập, chọn/đổi công ty, JWT, refresh, đổi mật khẩu) | P0 | Tất cả |
| CMN-02 | Kiểm tra permission + data scope ở BE | P0 | Tất cả |
| CMN-03 | Audit log tự động (interceptor) | P0 | Tất cả |
| CMN-04 | Đánh số chứng từ (mã tự sinh theo mẫu) | P0 | WH, FIN, PAY, HRM | 
| CMN-05 | Đính kèm file (object storage) | P1 | HRM-02, FIN-06, WH-04… |
| CMN-06 | Import Excel (wizard: tải mẫu → kiểm tra → xem lỗi → ghi) chạy nền | P1 | Mọi chức năng có I |
| CMN-07 | Export Excel/PDF | P1 | Mọi chức năng có E |
| CMN-08 | Workflow engine (instance, step, lịch sử duyệt) | P1 | Mọi chức năng có W |
| CMN-09 | Khóa kỳ (period lock) | P1 | PAY-03/04/06, FIN-11, WH-09 |
| CMN-10 | Kiểm soát đồng thời (row version) + idempotency key | P0 | Chứng từ nghiệp vụ |

## Tổng số

| Phân hệ | Chức năng | P0 | P1 | P2 |
|---|---|---|---|---|
| ADM | 9 | 6 | 3 | 0 |
| HRM | 3 | 1 | 2 | 0 |
| PAY | 6 | 0 | 3 | 3 |
| FIN | 12 | 0 | 5 | 7 |
| WH | 9 | 0 | 7 | 2 |
| PRJ | 1 | 1 | 0 | 0 |
| **Cộng** | **40** | **8** | **20** | **12** |

ADM-08 tính là P0 (phần ghi log).
