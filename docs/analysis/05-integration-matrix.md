# 05 — Ma trận tích hợp giữa các phân hệ

## 1. Cơ chế tích hợp đề xuất **[P]**

Repo là modular monolith, một database **[C]**. Hai cơ chế:

| Cơ chế | Khi dùng | Cách làm |
|---|---|---|
| **Đọc qua query công khai** | Module B cần tra cứu dữ liệu chủ của A (nhân viên, dự án, vật tư) | A công khai query/interface trong Application; B gọi, không join bảng của A |
| **Sự kiện tích hợp (outbox)** | Hành động ở A phải sinh dữ liệu ở B (khóa bảng lương → chi phí nhân công) | A ghi sự kiện vào bảng outbox **cùng transaction** với thay đổi; job nền phát cho handler của B; handler idempotent theo `(source_type, source_id, version)` |

Không gọi trực tiếp handler của module khác trong cùng transaction cho luồng sinh dữ liệu chéo — lỗi ở B không được làm hỏng thao tác chốt ở A, và phải thử lại được **[P]**. Hiện BuildingBlocks có `PublishDomainEventsInterceptor` (sự kiện trong một module); outbox là phần cần thêm → `Q-ARC-06`.

Mỗi bản ghi sinh từ tích hợp lưu: `source_module`, `source_type`, `source_id`, `source_version`, `sync_status` (Pending/Done/Failed), `sync_error`.

## 2. Ma trận

| ID | Nguồn → Đích | Dữ liệu | Thời điểm | Cơ chế | Chống trùng / đảo | Trạng thái |
|---|---|---|---|---|---|---|
| INT-01a | ADM → tất cả | User, Role, Permission, DataScope | Mọi request | Đọc (claims + cache ngắn) | — | **[C]** |
| INT-01b | ADM → PAY/FIN/WH | Workflow: tạo instance khi submit, trả kết quả duyệt | Submit / Approve / Reject | Gọi service workflow (cùng transaction với chứng từ) | 1 instance mở cho 1 chứng từ | **[P]** |
| INT-01c | Tất cả → ADM | Thông báo, audit log | Sau sự kiện | Outbox (thông báo); interceptor (audit) | — | **[P]** |
| INT-01d | Tất cả ↔ ADM-05 | Danh mục dùng chung | Khi chọn | Đọc | Danh mục ngừng dùng vẫn hiển thị trên chứng từ cũ | **[C]** |
| INT-02a | HRM → PAY | Nhân viên, đơn vị, chức danh, trạng thái, `EmployeeAssignment` theo ngày | Tạo kỳ công/lương | Đọc + snapshot vào dòng bảng công/lương | — | **[C]** |
| INT-02b | HRM → ADM | Liên kết User ↔ Employee; phòng ban cho data scope | Khi gán / khi điều chuyển | Đọc | 1 nhân viên ↔ tối đa 1 user **[A]** | **[C]** |
| INT-02c | HRM → PAY | Nhân viên nghỉ việc | Khi đổi trạng thái | Không gửi thông báo (DEC-019); Chấm công – Tính lương tự đọc trạng thái khi lập bảng công | — | **[C]** |
| INT-03a | PAY → FIN | Chi phí nhân công (theo phòng ban/dự án, thành phần) | Khi bảng lương **Locked** | Outbox → `CostEntry(cost_type=labor)` | Unique `(payroll_id, run_no)`; mở khóa + tính lại → sinh bút đảo + bản mới, không sửa đè | **[P]** |
| INT-03b | PAY → FIN | Nghĩa vụ phải trả lương (từng NV hoặc tổng) | Khi bảng lương Locked | Outbox → công nợ phải trả / đề nghị chi lương | Như trên | **[U]** `Q-PAY-08` |
| INT-03c | FIN → PAY | Đã chi lương, tạm ứng đã khấu trừ | Khi quyết toán | Đọc | — | **[P]** |
| INT-04a | WH → FIN | Đơn mua được duyệt (cam kết chi) | Approved | Đọc (FIN-01 so ngân sách) | — | **[P]** |
| INT-04b | WH → FIN | Phiếu nhập mua đã xác nhận → phải trả NCC | Confirmed | Outbox → `DebtLedgerEntry(payable)` | Unique theo phiếu nhập; đảo phiếu → bút đảo | **[U]** công nợ ghi theo phiếu nhập hay hóa đơn? `Q-FIN-06` |
| INT-04c | WH → FIN | Phiếu xuất cho dự án → chi phí vật tư | Confirmed | Outbox → `CostEntry(cost_type=material)` | Unique theo dòng phiếu xuất | **[U]** giá xuất kho `Q-WH-01`; xuất = chi phí? `Q-FIN-03` |
| INT-04d | WH → FIN | Giá trị tồn kho | Báo cáo | Đọc | — | **[U]** `Q-WH-01` |
| INT-05a | PRJ → tất cả | Project ID, trạng thái, công ty, đơn vị phụ trách | Mọi giao dịch gắn dự án | Đọc | Dự án ngừng hoạt động không chọn được cho giao dịch mới | **[C]** |
| INT-05b | PRJ → ADM | Danh sách dự án cho scope `project` | Gán scope | Đọc | — | **[C]** |
| INT-05c | Tất cả → PRJ | Tab dữ liệu liên kết của dự án | Xem chi tiết dự án | Đọc query công khai của từng module | Chỉ đọc | **[P]** |
| INT-06 | FIN-06/08 → FIN-07/10 | Lần chi, hoàn ứng → công nợ, dòng tiền thực | Khi ghi nhận chi/thu | Trong cùng module | Unique theo `Payment.id` | **[P]** |
| INT-07 | FIN-02…09 → FIN-11 | Sinh bút toán từ nghiệp vụ nguồn | Khi chốt | Trong cùng module | Unique `(source_type, source_id)` | **[U]** `Q-FIN-01`, `Q-FIN-02` |

## 3. Đối soát

| ID | Cặp | Công thức kiểm tra | Hiển thị ở |
|---|---|---|---|
| REC-01 | PAY ↔ FIN | Σ net lương kỳ (Locked) = Σ phải trả lương ghi nhận ở FIN; Σ phải trả − Σ đã chi = còn phải trả (PAY-06) | PAY-06-02 |
| REC-02 | PAY ↔ FIN | Σ chi phí nhân công kỳ ở FIN = Σ thành phần chi phí của bảng lương Locked | FIN-03-01 (cảnh báo lệch) |
| REC-03 | WH ↔ FIN | Σ giá trị phiếu nhập mua đã xác nhận = Σ phải trả NCC ghi nhận (chưa tính điều chỉnh) | FIN-07-03 |
| REC-04 | WH ↔ FIN | Σ giá trị xuất cho dự án = Σ `CostEntry(material)` theo dự án/kỳ | FIN-02-01 |
| REC-05 | WH nội bộ | Tồn cuối theo `StockLedgerEntry` = tồn trên snapshot (nếu có); hàng đi đường = Σ đã gửi − Σ đã nhận của phiếu chuyển chưa đóng | WH-08 |
| REC-06 | FIN ↔ PRJ | Mọi `CostEntry`, `Budget`, `Revenue`, `PaymentRequest` có `project_id` trỏ tới dự án tồn tại cùng tenant | Job kiểm tra + báo cáo |
| REC-07 | ADM ↔ HRM | User liên kết Employee nghỉ việc tự khoá (DEC-018); còn Active (quản trị cuối cùng) → cảnh báo | ADM-01-01 (cột cảnh báo) |
| REC-08 | Tích hợp | Số sự kiện outbox `Failed` hoặc `Pending` quá 15 phút **[P]** | Màn hình quản trị (ADM) |

Lệch đối soát **không tự sửa**; hiển thị để người có quyền xử lý bằng chứng từ điều chỉnh **[P]**.
