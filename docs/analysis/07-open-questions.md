# 07 — Câu hỏi mở và Decision Log

**Chặn** = chưa trả lời thì không viết được đặc tả hoặc không bắt đầu được chức năng đó.
Khi PO trả lời: chuyển dòng sang §2 Decision Log, ghi ngày + người quyết định.

## 1. Câu hỏi mở

### Kiến trúc / nền tảng

| ID | Câu hỏi | Ảnh hưởng | Phương án / đề xuất | Chặn |
|---|---|---|---|---|
| Q-ARC-03 | Thư viện biểu đồ cho dashboard? | Rule cấm thêm thư viện UI | `@ant-design/charts` (cùng hệ antd) · ECharts · Recharts | Không (cần trước dashboard) |
| Q-ARC-05 | Có cần Dark Mode không? shared-ui có hỗ trợ không? | Design token | Đề xuất hoãn tới khi xác nhận shared-ui hỗ trợ | Không |
| Q-ARC-06 | Đồng ý thêm cơ chế outbox + job nền (Hangfire/Quartz/BackgroundService) không? | Tích hợp PAY→FIN, WH→FIN, import lớn, thông báo | Đề xuất `BackgroundService` + bảng outbox tự viết trước; Hangfire khi cần lập lịch | Không (cần trước INT-03) |
| ~~Q-ARC-07~~ | ~~Import Excel có lỗi~~ → DEC-021: người dùng chọn ghi dòng hợp lệ hoặc huỷ, tải được chi tiết lỗi | BR-GEN-11 | — | — |
| Q-ARC-08 | Object storage dùng gì (MinIO tự host, S3, Azure Blob)? Giới hạn dung lượng/loại file? | CMN-05 | Đề xuất MinIO cho dev, abstraction để đổi | Không |
| Q-ARC-09 | Đăng nhập: chỉ username/mật khẩu, hay cần SSO / OTP / 2FA? | CMN-01 | Đề xuất username/mật khẩu trước | Không |

### ADM

| ID | Câu hỏi | Chặn |
|---|---|---|
| Q-ADM-01 | Danh sách vai trò mẫu và quyền mặc định ([04](04-permission-matrix.md) §4–5) có đúng thực tế doanh nghiệp không? | Không |
| Q-ADM-03 | Có cho gán scope trực tiếp cho user (ngoài role) không? | Không |
| Q-ADM-05 | Người gửi có được **thu hồi** sau khi đã có người duyệt bước 1 không? | Không (cần trước ADM-06) |
| Q-ADM-06 | Có **ủy quyền** duyệt (vắng mặt) không? | Không |
| Q-ADM-07 | Có **duyệt song song** (nhiều người cùng bước: tất cả/ một người đủ) không? | Không |
| Q-ADM-08 | Người tạo có được tự duyệt chứng từ của mình không (vd. giám đốc tự lập đề nghị)? | Không |
| Q-ADM-09 | Chính sách mật khẩu, thời gian phiên, số lần sai thì khóa? | Không |
| Q-ADM-10 | Kênh thông báo: chỉ trong app hay thêm email/Zalo? | Không |

### HRM

| ID | Câu hỏi | Chặn |
|---|---|---|
| Q-HRM-01 | Vị trí công việc và chức danh là **một** danh mục hay **hai**? Nhân viên có một hay nhiều chức danh? | **Có** (HRM-03) |
| Q-HRM-02 | Loại đơn vị cần có (Khối, Phòng, Ban, Đội, Ban điều hành dự án, Công trường…)? | Không |
| Q-HRM-03 | Có cần ghi nhân viên thuộc dự án nào (để chấm công / phân bổ lương theo dự án) không? Ghi ở HRM hay ở PAY (theo ngày công)? | **Có** (PAY-03, FIN-03) |
| Q-HRM-04 | Loại nhân sự (chính thức, thời vụ, khoán, công nhân thuê ngoài theo tổ đội)? Công nhân thuê ngoài có hồ sơ nhân viên không? | **Có** (HRM-02, PAY) |
| Q-HRM-05 | Trường thông tin định danh nào bắt buộc (CCCD, MST cá nhân, số BHXH)? | Không |

### PAY

| ID | Câu hỏi | Chặn |
|---|---|---|
| Q-PAY-01 | Nguồn chấm công: nhập tay, Excel, máy chấm công (hãng nào, API/file), app mobile GPS? | Không (P1 làm nhập tay + Excel) |
| Q-PAY-02 | Ca qua ngày tính công cho ngày nào? | **Có** (PAY-01) |
| Q-PAY-03 | Danh sách thành phần lương (lương cơ bản, lương đóng BH, phụ cấp chức vụ/công trường/xăng xe/điện thoại, thưởng, khoán…) | **Có** (PAY-04) |
| Q-PAY-04 | Hệ số tăng ca ngày thường / nghỉ tuần / lễ / ca đêm; có trần giờ tăng ca không? | **Có** (PAY-03, PAY-04) |
| Q-PAY-05 | Công tiêu chuẩn tháng: cố định (26) hay theo lịch thực tế? | **Có** (PAY-04) |
| Q-PAY-06 | Cách tính BHXH/BHYT/BHTN (mức đóng, trần, tỷ lệ NLĐ/DN) — cấu hình theo văn bản hiện hành, ai duy trì? | **Có** (PAY-04) |
| Q-PAY-07 | Thuế TNCN: lũy tiến từng phần, giảm trừ gia cảnh, người phụ thuộc lấy từ đâu? Khấu trừ 10% với lao động thời vụ? | **Có** (PAY-04) |
| Q-PAY-08 | Lương chuyển sang FIN dạng gì: một đề nghị chi lương tổng, hay công nợ phải trả từng nhân viên? | **Có** (INT-03b, PAY-06) |
| Q-PAY-09 | Sai sót phát hiện sau khi khóa: mở lại kỳ hay truy lĩnh/truy thu kỳ sau? | Không |
| Q-PAY-10 | Thời điểm chốt công và chốt lương hằng tháng (ngày cố định?) | Không |

### FIN

| ID | Câu hỏi | Chặn |
|---|---|---|
| Q-FIN-01 | Chế độ kế toán áp dụng (TT 200 hay TT 133) và hệ thống tài khoản chi tiết? | **Có** (FIN-11) |
| Q-FIN-02 | Bảng định khoản tự động cho từng nghiệp vụ nguồn (nhập mua, xuất dự án, lương, chi tiền, tạm ứng) | **Có** (FIN-11) |
| Q-FIN-03 | Xuất kho cho dự án ghi nhận chi phí ngay (TK 621/154) hay chỉ khi có xác nhận tiêu hao? | **Có** (FIN-02) |
| Q-FIN-04 | Phương pháp phân bổ chi phí chung (theo doanh thu, theo chi phí trực tiếp, tỷ lệ cố định…) | **Có** (FIN-05 phần phân bổ) |
| Q-FIN-05 | Quy tắc ghi nhận doanh thu (theo hóa đơn, theo nghiệm thu, theo tiến độ hợp đồng)? Ai nhập? | **Có** (FIN-09) |
| Q-FIN-06 | Công nợ phải trả NCC ghi theo phiếu nhập hay theo hóa đơn? Có đối chiếu 3 chiều (đơn mua–nhập–hóa đơn) không? | **Có** (FIN-07) |
| Q-FIN-07 | Kỳ lương và kỳ kế toán có dùng chung một khái niệm kỳ/khóa kỳ không? | Không |
| Q-FIN-08 | Danh mục khách hàng/chủ đầu tư thuộc FIN hay ADM-05? Dùng chung với nhà cung cấp (đối tượng công nợ chung) không? | Không |
| Q-FIN-09 | "Nhóm chi phí" (ADM-05) và "Khoản mục chi phí" (FIN) là một hay hai danh mục? | **Có** (FIN-01) |
| Q-FIN-10 | Làm tròn tiền: đến đồng, đến nghìn? Ở bước nào? | Không |
| Q-FIN-11 | Vượt ngân sách: chặn, cảnh báo, hay cảnh báo + duyệt thêm cấp? | Không |
| Q-FIN-12 | Tuổi nợ tính từ ngày chứng từ hay hạn thanh toán? Các mốc (0–30, 31–60…)? | Không |
| Q-FIN-13 | Hạn mức duyệt theo số tiền cho đề nghị thanh toán/tạm ứng (vd. > 500 triệu cần TGĐ)? | Không (cấu hình ở ADM-06) |
| Q-FIN-14 | Có nhiều đơn vị tiền tệ không? | Không |

### WH

| ID | Câu hỏi | Chặn |
|---|---|---|
| Q-WH-01 | Phương pháp tính giá xuất kho: bình quân gia quyền (cuối kỳ / di động), FIFO, đích danh? | **Có** (giá trị tồn, FIN-02) |
| Q-WH-02 | Có cho phép tồn âm không? | Không |
| Q-WH-03 | Giữ chỗ tồn: khi duyệt yêu cầu vật tư, hay không giữ chỗ? | Không |
| Q-WH-04 | Thủ kho được phân quyền theo kho (gán trực tiếp) hay theo dự án? | Không |
| Q-WH-05 | Danh mục nhà cung cấp đặt ở WH hay ADM-05? | Không |
| Q-WH-06 | Có quản lý lô / số seri / hạn dùng không? (vật tư xây dựng thường không, trừ phụ gia, sơn) | Không |
| Q-WH-07 | Ván khuôn, đà giáo luân chuyển: xuất là tiêu hao hay cho mượn (thu hồi về kho)? | **Có** (WH-06) |

### PRJ

| ID | Câu hỏi | Chặn |
|---|---|---|
| ~~Q-PRJ-01~~ | ~~Danh mục trạng thái dự án~~ → DEC-021: Đang thực hiện, Hoàn thành, Đóng | — |
| ~~Q-PRJ-02~~ | ~~Có dự án con / gói thầu không?~~ → DEC-021: có thể có hoặc không | — |

## 2. Decision Log

| ID | Ngày | Quyết định | Người quyết định | Liên quan |
|---|---|---|---|---|
| DEC-001 | 29/10/2026 | Stack .NET 10 + EF Core + PostgreSQL + React/AntD/shared-ui, multi-tenant theo `tenant_id` | (ADR-0001 đã duyệt) | D-01, D-02 |
| DEC-002 | 29/10/2026 | Bản phân tích Giai đoạn 1 ghi vào `docs/analysis/` | Người dùng | — |
| DEC-003 | 29/10/2026 | Một tenant (khách hàng) có **nhiều pháp nhân** dùng chung hệ thống; **dữ liệu tách theo từng công ty** | PO | Q-ARC-01 → Q-ARC-10, 11, 12, 13 |
| DEC-004 | 29/10/2026 | Prototype **lai**: phần thêm/sửa danh mục, quy trình duyệt, kho chạy API + DB thật; lương và hạch toán dùng tham số mẫu, giao diện ghi rõ "chưa duyệt" | PO | Q-ARC-02 |
| DEC-005 | 29/10/2026 | Đăng nhập **không nhập mã công ty**. Sau khi đăng nhập, người dùng **chọn công ty** để làm việc; dữ liệu chạy theo công ty đã chọn. Hệ quả: username unique toàn nền tảng | PO | Q-ADM-04 |
| DEC-006 | 29/10/2026 | **Được dùng thư viện** lưới kiểu Excel (ngoại lệ rule `design-system.md`) | PO | Q-ARC-04 |
| DEC-007 | 29/10/2026 | Chọn thư viện lưới **mới**: AG Grid Community (MIT). Chỉ dùng cho màn hình lưới nhập liệu (Bảng công, Bảng lương, Ngân sách, Kiểm kê, Kế hoạch dòng tiền); bảng danh sách thường vẫn dùng `TableComponent` của shared-ui | PO (theo đề xuất) | Q-ARC-04b |
| DEC-008 | 29/10/2026 | Danh mục (vật tư, nhà cung cấp, khách hàng, chức danh, danh mục chung) **dùng chung cả tập đoàn** | PO | Q-ARC-10 |
| DEC-009 | 29/10/2026 | Một nhân viên **chỉ thuộc một công ty** tại một thời điểm | PO | Q-ARC-11 |
| DEC-010 | 29/10/2026 | Bản ghi thuộc phạm vi dữ liệu khi khớp **một trong** các thuộc tính phạm vi (dự án hoặc phòng ban…) | PO | Q-ADM-02 |
| DEC-011 | 29/10/2026 | **Ban lãnh đạo xem được dữ liệu tất cả công ty** (chế độ "Tất cả công ty", chỉ đọc) | PO | Q-ARC-12 |
| DEC-012 | 29/10/2026 | **Quản trị (tenant admin) tạo công ty mới** | PO | Q-ARC-13 |
| DEC-013 | 29/10/2026 | Viết ADR-0002 (nhiều công ty trong tenant) và cập nhật rule `multi-tenancy-and-security.md` | PO | D-06 |
| DEC-014 | 29/10/2026 | Thêm module `hrm`, `payroll` vào `project-architecture.md`; `accounting` đổi tên hiển thị thành "Tài chính – Kế toán" | PO | D-05 |
| DEC-015 | 29/10/2026 | Thêm scope `department-tree`, `project` (scope gắn role theo từng module) và action `import`, `submit`, `lock`, `view-sensitive` vào rule phân quyền; Reject gộp vào `approve` | PO | D-07, D-08 |
| DEC-016 | 29/10/2026 | **Không** điều chuyển nhân viên giữa các công ty | PO | Q-ARC-14 |
| DEC-017 | 29/10/2026 | Hệ thống tài khoản kế toán **dùng chung** mọi công ty (cấp tenant) | PO | Q-ARC-15 |
| DEC-018 | 30/10/2026 | Trả lời câu hỏi epic: từ chối bắt buộc lý do · nhắc chờ duyệt 08:00 hằng ngày tới khi xử lý · công ty có trạng thái Ngừng hoạt động · tài khoản tự khoá khi nhân viên nghỉ việc · nhân viên xem được thông tin nhạy cảm của mình · không cảnh báo tồn tối thiểu · nhận vượt đơn mua không giới hạn, cảnh báo + đánh dấu đỏ · yêu cầu mua không bắt buộc từ yêu cầu vật tư · dự án Hoàn thành không chọn được cho chứng từ mới · PM/Sprint để trống | PO | Epic ADM Q12/Q13, HRM Q11, WH Q10/Q11/Q12, PRJ Q05 |
| DEC-019 | 30/10/2026 | Xác nhận diễn giải DEC-018 (nhắc lần đầu 08:00 hôm sau; khoá tài khoản vào ngày nghỉ việc có hiệu lực, trừ quản trị cuối cùng; dự án Đóng như Hoàn thành, không ngoại lệ) · nhận hàng lệch số đặt (vượt **hoặc thiếu**) đều đánh dấu đỏ · công ty Ngừng hoạt động không chọn được để làm việc, Ban lãnh đạo xem ở chế độ Tất cả công ty · **không** gửi thông báo khi nhân viên nghỉ việc · ~~trạng thái dự án là danh mục dùng chung quản lý được~~ (huỷ, xem DEC-020) | PO | Epic ADM Q16, HRM Q09 |
| DEC-020 | 30/10/2026 | Làm rõ: "CRUD" là chức năng của Quản lý dự án — thêm mới, sửa, xoá, sao chép dự án. Huỷ phần "trạng thái dự án là danh mục" của DEC-019; Q-PRJ-01 mở lại | PO | Epic PRJ Q03 |
| DEC-021 | 30/10/2026 | Trạng thái dự án: **Đang thực hiện, Hoàn thành, Đóng** · dự án **có thể có** dự án con / gói thầu · sao chép dự án mang **toàn bộ thông tin trừ mã** · nhập Excel có lỗi: **người dùng chọn** ghi dòng hợp lệ hoặc huỷ, **tải được chi tiết lỗi** về máy (áp dụng chung mọi phân hệ) | PO | Q-PRJ-01, Q-PRJ-02, Q-ARC-07; Epic PRJ Q03/Q04/Q06/Q07 |
| DEC-022 | 30/10/2026 | PO yêu cầu làm liên tục mọi giai đoạn prototype (2.1 → 2.9) trên nhánh `poc`, **không dừng chờ duyệt đặc tả từng chức năng**; epic overview là căn cứ. Ngoại lệ có chủ ý với bước 1–2 của `feature-workflow.md` cho nhánh POC | PO | — |
