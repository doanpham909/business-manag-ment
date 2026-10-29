# ADR-0001: Công nghệ và mô hình multi-tenant

- **Trạng thái:** Đã duyệt
- **Ngày:** 29/10/2026

## Bối cảnh

Xây dựng ứng dụng web quản lý doanh nghiệp dạng SaaS cho nhiều công ty. Module đầu tiên: Quản trị hệ thống, Kho, Kế toán, Quản lý dự án. Đội phát triển đã quen stack của dự án QMS (.NET 8 + EF Core, React + Ant Design + `@systemvci/shared-ui`).

## Quyết định

| Hạng mục | Lựa chọn | Lý do |
|---|---|---|
| Backend | .NET 10 LTS, Clean Architecture + DDD + CQRS (MediatR) như QMS | Đội dùng lại kinh nghiệm và rules của QMS. .NET 8 hết hỗ trợ 10/11/2026 nên dùng bản LTS mới (hỗ trợ tới 11/2028) |
| ORM | EF Core 10 + Npgsql | Giống QMS |
| Database | PostgreSQL 18 | Theo yêu cầu dự án; mạnh về giao dịch, `jsonb`, chi phí license 0 |
| Frontend | React + TypeScript + Vite, Ant Design 6 + `@systemvci/shared-ui` + Tailwind | Giao diện thống nhất với các sản phẩm khác của công ty |
| Ngôn ngữ UI | Chỉ tiếng Việt | Không cần i18n framework (YAGNI); dùng locale `vi_VN` của antd và dayjs |
| Repo | Monorepo: `apps/api`, `apps/web` | Một PR thay đổi được cả API và UI; CI chung |
| Multi-tenant | Một database, cột `tenant_id` + EF global query filter | Đơn giản vận hành nhất, đủ cho quy mô SME. Có thể tách tenant lớn sang DB riêng sau nếu cần |

## Hệ quả

- FE và BE không chia sẻ type tự động. Khi cần, sinh TypeScript client từ OpenAPI của API.
- MediatR khóa ở v12 (v13+ là license thương mại). Không dùng AutoMapper (v15+ thương mại), dùng mapping tay hoặc projection `Select`.
- Mọi entity nghiệp vụ phải có `tenant_id`; sai sót ở đây là lỗ hổng rò rỉ dữ liệu giữa các công ty → có test và review riêng (xem `.claude/rules/multi-tenancy-and-security.md`).

## Phương án đã cân nhắc

- **NestJS + Prisma/TypeORM**: chung TypeScript FE/BE, nhưng đội phải học lại; không dùng được rules/kinh nghiệm EF Core của QMS.
- **Database riêng cho từng tenant**: cách ly mạnh nhất nhưng migration và vận hành phức tạp ngay từ đầu.
- **Tự xây design system (Tailwind + Radix)**: tự do tùy biến nhưng tốn công và khác giao diện QMS.
