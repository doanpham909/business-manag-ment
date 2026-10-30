# 08 — Kế hoạch xây dựng prototype

## 1. Cách làm prototype — **đã chọn phương án C** (DEC-004, 29/10/2026)

Master prompt yêu cầu prototype **tương tác thật** (tạo/sửa, validation, trạng thái, dữ liệu liên thông) và đồng thời cấm "hardcode dữ liệu demo trong luồng production".

| Phương án | Ưu | Nhược |
|---|---|---|
| **A. Mock phía FE** (MSW, dữ liệu trong bộ nhớ trình duyệt) | Dựng màn hình nhanh nhất; PO xem sớm | Toàn bộ logic trạng thái, scope, tích hợp phải viết 2 lần (mock rồi BE thật); dễ duyệt UI cho logic sai |
| **B. API thật ngay từ đầu** | Không làm lại; quy tắc nghiệp vụ, quyền, tenant được kiểm chứng sớm | Mỗi màn hình chậm hơn; phần chưa có công thức (lương, hạch toán) chưa làm được |
| **C. Lai** — đề xuất **[P]** | CRUD + quy trình duyệt + kho làm thật (B). Lương và hạch toán: BE trả kết quả tính bằng **bộ tham số mẫu đánh dấu rõ "DỮ LIỆU MẪU – CHƯA DUYỆT"**, thay khi có công thức | Cần kỷ luật đánh dấu phần mẫu |

Dữ liệu mẫu (tenant "Công ty CP Xây dựng Demo", 5 đơn vị, 3 dự án) nạp bằng **seeder chỉ chạy ở môi trường Development** **[P]**, không nằm trong migration, không người thật.

## 2. Giai đoạn

Thứ tự theo phụ thuộc dữ liệu (master prompt cho phép điều chỉnh). Khác thứ tự gốc: **PRJ-01 lên sớm** vì kho, chi phí, ngân sách đều cần dự án; **HRM-01 vào nền tảng** vì data scope cần phòng ban.

| GĐ | Nội dung | Chức năng | Điều kiện bắt đầu | Kết quả PO xem được |
|---|---|---|---|---|
| **2.0** | Chuẩn bị | ✅ ADR-0002 · ✅ module `hrm`, `payroll` · ✅ scope/action mở rộng · ✅ ngoại lệ AG Grid · ✅ epic overview 6 phân hệ (`docs/features/*/0_epic-overview.md`, trạng thái Nháp). Còn: PO duyệt epic; ADR outbox (trước GĐ 2.8) | — | Tài liệu |
| **2.1** ✅ code | Nền tảng | CMN-01 đăng nhập + chọn/đổi công ty · tầng công ty (ADR-0002 §5) · CMN-02 permission + scope · CMN-03 audit · CMN-04 đánh số · CMN-10 concurrency/idempotency · shell (menu theo quyền, header) · wrapper dùng chung (DataGrid, FilterBar, FormDrawer, StatusTag, ApprovalTimeline, AuditHistory) | Không còn câu hỏi chặn | Đăng nhập → chọn công ty → menu ẩn/hiện theo quyền; đổi công ty ở header; chế độ "Tất cả công ty" |
| **2.2** ✅ code | Admin + tổ chức + dự án | ADM-00, 01, 02, 03, 05 · HRM-01 · PRJ-01 · ADM-08 (màn hình xem) | `Q-HRM-02` | Tạo user, vai trò, scope; cây tổ chức; danh mục dự án; xem audit |
| **2.3** | Nhân sự | HRM-02, HRM-03 · CMN-05 đính kèm · CMN-06/07 import/export | `Q-HRM-01`, `Q-HRM-04` | Hồ sơ 5 tab, import nhân viên |
| **2.4** | Workflow + thông báo | ADM-06, ADM-07, ADM-04 · CMN-08 · hộp việc chờ duyệt | `Q-ADM-05…08` | Cấu hình quy trình, gửi/duyệt/từ chối trên 1 chứng từ thử |
| **2.5** | Kho | WH-01, 02, 03, 04, 05, 06, 08 (tồn theo số lượng) · sau đó WH-07, WH-09 | `Q-WH-03`, `Q-WH-05`, `Q-WH-07` | Luồng yêu cầu → mua → nhập → xuất → tồn, thẻ kho |
| **2.6** | Chấm công | PAY-01, 02, 03 · CMN-09 khóa kỳ | `Q-PAY-02`, `Q-PAY-04`, `Q-HRM-03` | Lịch ca, nhập/import công, bảng công tháng, duyệt + khóa |
| **2.7** | Tài chính P1 | FIN-06, FIN-08, FIN-04, FIN-05 (ghi nhận, chưa phân bổ), FIN-01 | `Q-FIN-09`, `Q-FIN-13` | Đề nghị thanh toán + chi từng phần, tạm ứng, ngân sách |
| **2.8** | Tính lương | PAY-04, 05, 06 · INT-03 (outbox) | `Q-PAY-03…08`, `Q-ARC-06` | Bảng lương, phiếu lương, chuyển chi phí sang FIN |
| **2.9** | Tài chính P2 | FIN-02, 03, 07, 09, 10, 11, 12 · INT-04 · đối soát REC-01…08 · dashboard | `Q-FIN-01…06`, `Q-WH-01`, `Q-ARC-03` | Công nợ, doanh thu, dòng tiền, hạch toán, báo cáo |

Có thể chạy song song: 2.5 (Kho) với 2.6 (Chấm công) sau khi 2.4 xong, nếu đủ người và câu hỏi đã có trả lời.

## 3. Chu trình mỗi chức năng (theo `feature-workflow.md`)

1. Đặc tả `docs/features/{module}/{STT}_{ten}.md` (skill `ba-skill`) → **PO duyệt**
2. Thiết kế kỹ thuật (entity, API, permission, màn hình) → **PO duyệt**
3. Backend: Domain → Persistence + migration → Application → Presentation + unit test
4. Frontend: types → API hook → trang/form → route/menu
5. Kiểm thử: build + test + lint BE/FE xanh, tự kiểm từng AC trên giao diện chạy local
6. Báo cáo theo mẫu ở master prompt Phần XV, PR (không tự merge)

## 4. Tiêu chí hoàn thành một giai đoạn prototype

- Mọi màn hình trong giai đoạn mở được từ menu và hoạt động với dữ liệu mẫu.
- Tạo/sửa/lọc/tìm/đổi trạng thái chạy thật; không có nút vô tác dụng. Chức năng chưa làm hiển thị nhãn "Chưa triển khai".
- Kiểm thử quyền: user không có quyền nhận 403 ở API (không chỉ bị ẩn nút); user khác tenant không thấy dữ liệu.
- `dotnet build -warnaserror`, `dotnet test`, `npm run lint`, `typecheck`, `test:run`, `build` xanh — báo kết quả thật.
- Phần dùng tham số mẫu chưa duyệt được đánh dấu trên giao diện.

## 5. Rủi ro chính

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Công thức lương/thuế/BH chưa có → PAY-04 trễ | Cao | Đẩy PAY-04 về 2.8; làm PAY-01…03 trước; tham số hóa, không viết cứng |
| Chính sách kế toán chưa có → FIN-11, báo cáo trễ | Cao | FIN-11 cuối cùng; các chi phí ghi `CostEntry` trước, bút toán sinh sau |
| Lọc theo công ty bị sót ở một truy vấn → lộ dữ liệu giữa các công ty | Cao | Global filter cho `company_id` giống `tenant_id`; test kiến trúc kiểm entity nghiệp vụ có `company_id`; test tích hợp với 2 công ty |
| AG Grid lệch giao diện shared-ui/antd | Trung bình | Spike 1–2 ngày trên PAY-03: theme AG Grid theo token của shared-ui, gói trong một wrapper `editable-grid` duy nhất |
| Scope dữ liệu 6 mức làm query chậm | Trung bình | Materialized path cho cây đơn vị; index `(tenant_id, project_id)`, `(tenant_id, department_id)` |
| Phạm vi trôi (hợp đồng, tiến độ, nghiệm thu) | Trung bình | Chỉ ghi vào danh sách đề xuất mở rộng, không đưa vào sidebar |

## 6. Đề xuất mở rộng (ngoài phạm vi, chỉ ghi nhận)

Hợp đồng lao động · tuyển dụng · đào tạo · nghỉ phép độc lập · BOQ/WBS · tiến độ · sản lượng · nghiệm thu · thanh toán hợp đồng xây dựng · BIM · quản lý thiết bị/bảo dưỡng · hợp đồng mua hàng · app chấm công mobile.
