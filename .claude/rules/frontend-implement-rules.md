---
paths:
  - "apps/web/**"
---

# Quy tắc frontend (`apps/web`)

React 19 + TypeScript + Vite. Server state: TanStack Query. Form: Formik + Yup. Client state: Zustand. Routing: React Router 7. Giao diện: xem `design-system.md`.

## Cấu trúc

```
src/
├── app/          providers, router (route-config.ts = nguồn route DUY NHẤT), layouts/main-layout
├── features/     {module}/ : {module}-routes.tsx, api/, components/, pages/, (hooks/, schemas/, types/ khi cần)
├── shared/       api/ (http-client, query-client), components/, constants/ (ROUTE_PATHS), types/, utils/
├── styles/       index.css (Tailwind layers)
└── test/         setup, render-with-providers
```

- `shared` **không** import `features`. Feature **không** import feature khác; cần dùng chung thì chuyển lên `shared`.
- Tên file kebab-case: `warehouse-list-page.tsx`, `use-warehouses-query.ts`, `warehouse-form-schema.ts`. Component PascalCase, hook `useXxx`.
- File ≤ 200 dòng. Trang lớn tách thành component con trong `features/{module}/components/`.
- Import bằng alias `@/` (không dùng `../../..`).

## Thêm một trang

1. Path vào `shared/constants/route-paths.ts` (slug tiếng Việt không dấu, ví dụ `/kho/danh-sach-kho`).
2. Route vào `features/{module}/{module}-routes.tsx` (`title`, `icon`, `page: () => import(...)`, `children`, `hideInMenu`). Menu, breadcrumb và router tự sinh từ đây.
3. Module mới: thêm mảng route vào `app/router/route-config.ts` và cập nhật bảng module trong `project-architecture.md`.

## Gọi API

```ts
// features/inventory/api/warehouse-api.ts — hàm gọi API thuần, có type
// PagedResult<T> import từ '@/shared/api/api-envelope'
export const warehouseApi = {
  list: (params: WarehouseListParams) => apiClient.get<PagedResult<Warehouse>>('/v1/inventory/warehouses', { params }),
  create: (body: CreateWarehouseInput) => apiClient.post<string>('/v1/inventory/warehouses', body)
}

// features/inventory/api/use-warehouses-query.ts — hook TanStack Query
export const warehouseKeys = {
  all: ['inventory', 'warehouses'] as const,
  list: (params: WarehouseListParams) => [...warehouseKeys.all, 'list', params] as const
}
export const useWarehousesQuery = (params: WarehouseListParams) =>
  useQuery({ queryKey: warehouseKeys.list(params), queryFn: () => warehouseApi.list(params) })
```

- **Luôn** qua `apiClient` (tự bóc `value`, lỗi thành `ApiError`). Không gọi `axios`/`fetch` trực tiếp.
- Query key bắt đầu bằng `[module, resource]`. Sau mutation thành công: `invalidateQueries({ queryKey: xxxKeys.all })`.
- **Không** lưu dữ liệu server vào `useState`/Zustand. Zustand chỉ cho state giao diện (ví dụ: sidebar thu gọn).
- Mẫu tham khảo: `features/dashboard/` (gọi `/v1/system/info`, xử lý loading/lỗi/dữ liệu).

## Form

- Formik + schema Yup đặt ở `features/{module}/schemas/`, message tiếng Việt.
- Lỗi `VALIDATION_ERROR` từ API (`ApiError.details`) gán vào field bằng `setFieldError`.
- Nút submit có trạng thái `loading` và bị khóa khi đang gửi.

## Trạng thái giao diện

Mỗi màn hình dữ liệu xử lý đủ 4 trạng thái: **đang tải** (skeleton), **lỗi** (`ErrorState` + nút thử lại), **rỗng** (`EmptyStateComponent`), **có dữ liệu**.

## Phân quyền ở FE

Khi module xác thực xong: ẩn menu/nút theo permission (`filterMenuByPermission` của shared-ui cho menu). Chỉ để trải nghiệm tốt hơn; backend vẫn là nơi kiểm tra thật.

## Định dạng hiển thị

- Ngày: `DD/MM/YYYY`, ngày giờ `DD/MM/YYYY HH:mm` (`formatDateTime` trong `shared/utils/format-date.ts`; cần định dạng mới thì thêm hàm vào file này).
- Tiền: `1.234.567 ₫` (`Intl.NumberFormat('vi-VN')`). Số lượng: tối đa 4 chữ số thập phân.
