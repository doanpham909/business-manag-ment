---
paths:
  - "apps/web/**"
---

# Design System

Giống QMS: **`@systemvci/shared-ui`** (bộ component chung của công ty) + **Ant Design 6** + **Tailwind 3.4** (preset của shared-ui). Mục tiêu: giao diện thống nhất giữa các sản phẩm.

## Thứ tự ưu tiên khi dựng UI

1. Component của `@systemvci/shared-ui`.
2. Component trong `apps/web/src/shared/components/` (wrapper riêng của dự án).
3. Ant Design, **chỉ khi** shared-ui không có bản tương ứng.
4. Tự viết bằng HTML + Tailwind, đặt trong feature. Dùng ở ≥ 2 module thì chuyển lên `shared/components`.

**Không** import trực tiếp từ `antd` các component shared-ui đã có bản bọc (bảng dưới). Không thêm thư viện UI/icon khác; icon dùng `lucide-react`.

## Catalog shared-ui (0.1.9)

| Nhu cầu | Dùng | Thay cho antd |
|---|---|---|
| Nút chính / phụ | `PrimaryButtonComponent` / `DefaultButtonComponent` (`ButtonComponent` cho biến thể khác) | `Button` |
| Ô nhập, ô tìm kiếm | `InputIconComponent` (`prefixIcon`/`suffixIcon`) | `Input` |
| Chọn 1 / nhiều / cây | `SelectComponent` / `MultiSelectComponent` / `TreeSelectComponent` | `Select`, `TreeSelect` |
| Chọn có tải từ API | `AsyncSelectComponent` / `AsyncMultiSelectComponent` | — |
| Ngày / khoảng ngày | `DatePickerComponent` / `RangePickerComponent` | `DatePicker` |
| Giờ / khoảng giờ | `TimePickerComponent` / `TimeRangePickerComponent` | `TimePicker` |
| Bật/tắt | `SwitchComponent` | `Switch` |
| Lỗi field Formik | `FormikErrorMessage` | — |
| Bảng, phân trang, chọn cột | `TableComponent`, `TablePaginationComponent`, `TableConfigColumnComponent` | `Table` |
| Tab | `TabsComponent` | `Tabs` |
| Drawer | `DrawerComponent` | `Drawer` |
| Hộp xác nhận | `ModalConfirmComponent` | `Modal.confirm` |
| Upload file / import Excel | `UploadComponent`, `UploadDraggerComponent`, `UploadFileModalComponent`, `ImportResultModalComponent` | `Upload` |
| Tiêu đề trang | `PageHeaderComponent` | `PageHeader` |
| Breadcrumb | `BreadcrumbComponent` | `Breadcrumb` |
| Trạng thái | `StatusTagComponent` (+ `DEFAULT_STATUS_COLOR_MAP`) | `Tag` |
| Chữ | `TypographyComponent` (`variant`) | `Typography` |
| Dữ liệu rỗng | `EmptyStateComponent` | `Empty` |
| Đang tải | `SkeletonTableComponent`, `SkeletonFormComponent`, `SkeletonComponent` | `Skeleton` |
| Lịch sử thay đổi | `HistoryTimelineComponent` | `Timeline` |
| Thông báo | `useAntNotification()` | `notification.*`, `message.*` |
| Theme antd | `getAntTheme(isDark)` (đã gắn trong `app/providers`) | — |
| Lọc menu theo quyền | `filterMenuByPermission(items, granted)` | — |

Antd được dùng trực tiếp khi shared-ui chưa có: `InputNumber`, `Form` layout, `Row/Col`, `Card`, `Descriptions`, `Tooltip`, `Popover`, `Dropdown`, `Layout`, `Menu`.

Khi nâng phiên bản shared-ui, xem export mới trong `index.d.ts` của gói và cập nhật bảng này.

## Màu sắc, chữ, khoảng cách

- Màu dùng biến CSS / class từ preset: `var(--color-*)`, `bg-main`, `text-main`, `border-divider`, `text-primary`… **Không** viết mã hex trong component.
- Chữ nội dung dùng `TypographyComponent`, không tự đặt `text-sm`/`font-bold` cho nội dung.
- Khoảng cách theo thang Tailwind (`gap-2`, `p-4`, `mb-6`…). Không dùng `style={{ margin: 13 }}`.
- Tailwind chạy với `preflight: false` để không đè style của antd. Không bật lại.

## Bố cục chuẩn

- **Trang danh sách:** `PageContainer` (`shared/components`, đã gồm `PageHeaderComponent`: `title` + `actions` = nút "Thêm mới") → thanh lọc (tìm kiếm, select) → `TableComponent` + phân trang.
- **Form tạo/sửa:** `DrawerComponent` cho form ngắn (≤ 8 trường); trang riêng cho form dài hoặc có bảng chi tiết (chứng từ kho, kế toán).
- **Trang chi tiết:** `PageHeaderComponent` (+ `StatusTagComponent`) → `Descriptions`/`TabsComponent` → `HistoryTimelineComponent`.
- Thao tác xóa/hủy/duyệt luôn qua `ModalConfirmComponent`.

## Lưu ý kỹ thuật

- shared-ui 0.1.9 import `antd/lib` (bản CommonJS). `vite.config.ts` có alias `antd/lib → antd` và Vitest inline gói này; **đừng xóa**.
- Build có cảnh báo `:global(...)` từ CSS của shared-ui: lỗi của gói, không ảnh hưởng chạy. Báo đội shared-ui sửa.
