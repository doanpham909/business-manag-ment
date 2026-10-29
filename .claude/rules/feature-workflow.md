# Quy trình làm một chức năng

Áp dụng cho mọi chức năng nghiệp vụ. Không bỏ bước, không code trước khi đặc tả được duyệt.

## 1. Phân tích yêu cầu → người dùng duyệt

- Đặc tả nằm ở `docs/features/{module}/`:
  - `0_epic-overview.md`: tổng quan module (mục tiêu, vai trò, quy trình, ma trận chức năng, trạng thái, phân quyền).
  - `{STT}_{ten-chuc-nang}.md`: mỗi chức năng một file (user story, quy tắc nghiệp vụ có mã `BR-xx`, tiêu chí chấp nhận `AC-xx` dạng Given/When/Then, phân quyền, màn hình).
- Dùng skill `ba-skill` (khung tài liệu BA thống nhất của công ty) nếu có.
- Liệt kê câu hỏi còn mở ở cuối đặc tả. **Dừng lại chờ duyệt.**

## 2. Thiết kế kỹ thuật → người dùng duyệt

Bổ sung vào đặc tả (mục "Thiết kế kỹ thuật"):

- Entity, bảng, cột, index, ràng buộc (theo `database-conventions.md`).
- API: method, route, request/response, mã lỗi (theo `api-conventions.md`).
- Permission mới (theo `multi-tenancy-and-security.md`).
- Màn hình: list/detail/form, component dùng lại từ shared-ui.

## 3. Code backend

Theo thứ tự trong `backend-implement-steps.md`: Domain → Persistence (+ migration) → Application → Presentation. Viết unit test cùng lúc.

## 4. Code frontend

Theo `frontend-implement-rules.md`: types → API hook → trang/form → đăng ký route/menu. Dùng component theo thứ tự ưu tiên trong `design-system.md`.

## 5. Kiểm thử

- Chạy lại toàn bộ test BE + FE, lint, build (xem `testing-rules.md`).
- Tự kiểm tra từng `AC-xx` trên giao diện thật (chạy API + web local). Ghi kết quả vào PR.

## 6. Pull Request

Theo `git-commit-rules.md`. Trong mô tả PR, liệt kê AC đã đáp ứng và những gì chưa làm.

## Khi yêu cầu thay đổi giữa chừng

Cập nhật đặc tả trước (ghi changelog), sau đó mới sửa code. Code và đặc tả phải luôn khớp nhau.
