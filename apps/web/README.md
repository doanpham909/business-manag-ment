# apps/web — Frontend "Quản lý doanh nghiệp"

React 19 + TypeScript + Vite 8, Ant Design 6 + `@systemvci/shared-ui` + Tailwind 3.4.3 (cùng design system với QMS),
TanStack Query v5 + axios, zustand, react-router v7 (library mode), Formik + Yup, Vitest + Testing Library.

## Chạy dự án

Yêu cầu Node >= 24 (xem `.nvmrc`), dùng **npm**.

```bash
cd apps/web
npm ci
cp .env.example .env.local   # tuỳ chọn: đổi VITE_API_PROXY_TARGET nếu backend không chạy ở :5080
npm run dev                  # http://localhost:4200
```

Khi dev, Vite proxy `/api` và `/health` sang `VITE_API_PROXY_TARGET` (mặc định `http://localhost:5080`, tức `apps/api`).

## Scripts

| Script                            | Mô tả                                      |
| --------------------------------- | ------------------------------------------ |
| `npm run dev`                     | Dev server cổng 4200                       |
| `npm run build`                   | Type-check (`tsc -b`) rồi build ra `dist/` |
| `npm run preview`                 | Xem thử bản build                          |
| `npm run lint` / `lint:fix`       | ESLint (flat config)                       |
| `npm run typecheck`               | Kiểm tra kiểu, không build                 |
| `npm run format` / `format:check` | Prettier                                   |
| `npm test` / `npm run test:run`   | Vitest (watch / chạy một lần)              |

## Cấu trúc thư mục

```
src/
├── main.tsx              # điểm vào: import CSS (antd reset → shared-ui → Tailwind), render <App/>
├── app/                  # lắp ráp ứng dụng
│   ├── providers/        # ConfigProvider (vi_VN + theme shared-ui), antd App, QueryClientProvider
│   ├── router/           # route-config.ts (NGUỒN DUY NHẤT của route/menu), route-objects, 404, error page
│   └── layouts/          # main-layout: sidebar menu, header, breadcrumb, <Outlet/>
├── features/<module>/    # mỗi module nghiệp vụ: <module>-routes.tsx, pages/, api/, components/
├── shared/
│   ├── api/              # http-client (axios + bóc envelope + chuẩn hoá lỗi), query-client
│   ├── components/       # component dùng chung (PageContainer, ErrorState, PlaceholderPage...)
│   ├── constants/        # ROUTE_PATHS, tên app
│   ├── types/ utils/     # kiểu & hàm tiện ích dùng chung
├── styles/               # index.css (Tailwind + token màu)
└── test/                 # setup Vitest, helper render
```

Quy ước: tên file kebab-case, mỗi file < 200 dòng, alias `@/` → `src/`. UI ưu tiên component của
`@systemvci/shared-ui` → Ant Design → HTML. Server state dùng TanStack Query (không `useState`), gọi API qua
`apiClient` trong `shared/api/http-client.ts` (không dùng axios trực tiếp). Màu dùng token `var(--color-*)` /
class Tailwind của preset shared-ui, không hardcode hex.

## Thêm trang cho một module

1. Thêm path vào `src/shared/constants/route-paths.ts`.
2. Tạo page `src/features/<module>/pages/<ten-trang>-page.tsx` với `export default`.
3. Khai báo trong `src/features/<module>/<module>-routes.tsx`:
   ```tsx
   { path: ROUTE_PATHS.xxx, title: 'Tên hiển thị', icon: <Icon size={18} />, page: () => import('./pages/xxx-page') }
   ```
   Nhóm menu con: dùng `children: [...]`; trang không hiện trên menu: `hideInMenu: true`.
4. Module mới: thêm `...<module>Routes` vào `src/app/router/route-config.ts`.

Router, menu sidebar và breadcrumb tự cập nhật; trang được lazy-load thành chunk riêng.

## Gọi API (mẫu: `features/dashboard`)

- `api/<x>-api.ts`: kiểu dữ liệu + hàm gọi `apiClient.get<T>(...)` — trả thẳng `value` của envelope.
- `api/use-<x>-query.ts`: hook `useQuery` + query key của feature.
- Component dùng hook và xử lý đủ 3 trạng thái: loading (Skeleton), lỗi (`ErrorState` + thử lại), dữ liệu.
