---
paths:
  - "apps/api/**"
  - "apps/web/src/**"
---

# Quy ước REST API

## Route

- Tiền tố `api/v1` (hằng `AppConstants.ApiV1Prefix`). Dạng: `/api/v1/{module}/{resource}` — chữ thường, kebab-case, resource số nhiều.
- Ví dụ: `GET /api/v1/inventory/warehouses`, `POST /api/v1/accounting/journal-entries/{id}/approve`.

| Thao tác | Method + route | Thành công |
|---|---|---|
| Danh sách (phân trang) | `GET /resources?pageIndex=1&pageSize=20&searchTerm=&sortColumn=&sortOrder=asc` | 200 |
| Chi tiết | `GET /resources/{id}` | 200 |
| Tạo | `POST /resources` | 200, `value` = id mới |
| Sửa | `PUT /resources/{id}` | 200 |
| Xóa (mềm) | `DELETE /resources/{id}` | 200 |
| Hành động nghiệp vụ | `POST /resources/{id}/{action}` (`approve`, `cancel`, `post`…) | 200 |

- `pageIndex` bắt đầu từ 1, `pageSize` mặc định 10, tối đa 100.
- Không có `tenantId` trong route/query/body. Tenant lấy từ token.

## Response thành công (envelope `Result<T>`)

```json
{ "isSuccess": true, "isFailure": false, "error": { "code": "", "message": "" }, "warningMessage": null, "value": { } }
```

Danh sách phân trang: `value = { items, pageIndex, pageSize, totalCount, hasNextPage, hasPreviousPage }`.

## Response lỗi (`ErrorResponse`)

```json
{ "code": "VALIDATION_ERROR", "type": "ValidationError", "title": "Validation Error", "status": 400,
  "detail": "Thông báo cho người dùng", "errors": [{ "code": "Name", "message": "..." }], "traceId": "..." }
```

| status | code |
|---|---|
| 400 | `VALIDATION_ERROR` (kèm `errors[]` theo field), `BAD_REQUEST` |
| 401 / 403 | `UNAUTHORIZED` (thêm khi làm module xác thực) / `FORBIDDEN` |
| 404 | `NOT_FOUND` hoặc mã riêng, ví dụ `WAREHOUSE_NOT_FOUND` |
| 409 | `CONFLICT` |
| 422 | `BUSINESS_RULE_VIOLATION` |
| 500 | `SERVER_ERROR` (không lộ chi tiết; dùng `traceId` để tra log) |

- `detail` là câu tiếng Việt hiển thị được ngay cho người dùng.
- Mã lỗi dùng chung khai báo trong `BuildingBlocks.Shared/Constants/ErrorCodes.cs`.

## Phía frontend

- Gọi qua `apiClient` (`apps/web/src/shared/api/http-client.ts`): tự bóc `value`, lỗi chuẩn hóa thành `ApiError { status, code, message, details, traceId }`.
- `errors[]` của 400 gắn vào field tương ứng của form Formik; lỗi khác hiển thị bằng notification.

## Tài liệu API

Swagger tại `http://localhost:5080/swagger` (chỉ môi trường Development). Endpoint mới phải có `ProducesResponseType` và XML comment ngắn.
