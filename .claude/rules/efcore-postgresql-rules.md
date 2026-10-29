---
paths:
  - "apps/api/**"
---

# Quy tắc viết EF Core (PostgreSQL)

Rút từ kinh nghiệm audit thực tế ở QMS. Các lỗi dưới đây **vẫn compile và trả kết quả đúng, chỉ chậm hoặc mất dữ liệu âm thầm**, nên review bằng mắt thường khó phát hiện.

Nền tảng có sẵn: `FindAll()` đã `AsNoTracking()` (đọc). `FindByIdAsync` / `FindSingleAsync` / `FindAllTracking` có tracking (load rồi sửa). `TransactionPipelineBehavior` tự `SaveChangesAsync` cho mọi command. `ToPagedResultAsync()` đếm và phân trang ở DB.

## A. Query

1. **Giữ `IQueryable` tới cuối.** `Where`, `OrderBy`, `Skip/Take`, `Count`, `GroupBy`, `Sum` phải chạy trước `ToListAsync()`. Không kéo cả bảng về rồi lọc trong bộ nhớ.
2. **Không đoán "EF không dịch được".** Nghi ngờ thì in `query.ToQueryString()` xem SQL thật. `statuses.Contains(x.Status)` với enum lưu chuỗi vẫn dịch thành `= ANY(...)`.
3. **Projection trước khi lấy dữ liệu:** `.Select(x => new XResponse(...))` để SQL chỉ lấy cột cần.
4. **Query chỉ đọc dùng `FindAll`**, không dùng bản tracking.
5. Xử lý trong bộ nhớ chỉ khi dữ liệu đến từ nguồn ngoài DB, và làm **sau khi đã phân trang**.
6. Tìm kiếm không dấu/không phân biệt hoa thường: `EF.Functions.ILike(x.Name, $"%{term}%")`. Bảng lớn cần tìm theo tên thì thêm extension `unaccent` + index `pg_trgm` (ghi trong đặc tả).

## B. Command

1. **Ghi hàng loạt** dùng `AddRangeAsync` / `UpdateRange` / `RemoveRange`, không lặp từng dòng.
2. **Không query trong vòng lặp (N+1):** lấy một lần bằng `ids.Contains(x.Id)` rồi tra `Dictionary`.
3. **Kiểm tra tồn tại** bằng `AnyAsync(...)`.
4. **Không tự gọi `SaveChangesAsync`**, tuyệt đối không gọi trong vòng lặp. Chỉ command có `[SkipTransaction]` mới tự gọi.
5. **Entity cần sửa phải được track** (`FindByIdAsync` / `FindAllTracking`). Sửa entity lấy từ `FindAll` sẽ **không được lưu** và không báo lỗi; gọi `repository.Update/Remove` với entity không track sẽ ném lỗi (chống ghi chéo tenant).
6. **Entity con mới bị hiểu nhầm là `Modified`:** do Id đã gán sẵn. Thêm tường minh bằng repository `AddAsync`/`AddRangeAsync` hoặc qua navigation của entity đang tracked. **Cấm** dùng `GetDatabaseValuesAsync` trong vòng lặp ChangeTracker để chữa.
7. **Collection "nhật ký" không `Include` sẽ mất dữ liệu âm thầm.** Nếu method domain tự thêm dòng vào `History`/`Timeline`/`AuditLog` mà query không `Include` collection đó, dòng mới không được insert. Cách làm: đọc source từng method domain được gọi, với mỗi loại entity con có thể phát sinh thì inject repository riêng và `AddRangeAsync` tường minh sau khi gọi method.
8. Chứng từ kế toán/kho sửa đồng thời: dùng concurrency token `xmin`. Bắt `DbUpdateConcurrencyException` và báo người dùng tải lại dữ liệu.

## C. SQL thô (chỉ khi thật cần: báo cáo, tổng hợp nặng)

- Ưu tiên `Database.SqlQuery<T>(...)` / `FromSql(...)` trong EF trước khi cân nhắc Dapper.
- **Không có global filter** → tự thêm `tenant_id = @tenantId` và `is_deleted = false` cho **mọi** bảng trong JOIN/subquery.
- Giá trị luôn qua tham số. `ORDER BY` chỉ nhận cột trong whitelist.
- Phân trang sâu: lấy trang id trước bằng CTE rồi JOIN lại để tính cột nặng.

## D. Checklist review (PR có đụng EF)

- [ ] Không có `ToListAsync()` trước `Where/Skip/Take/GroupBy/Sum/Count`
- [ ] Danh sách dùng `ToPagedResultAsync` và projection `Select`
- [ ] Query chỉ đọc dùng `FindAll`
- [ ] Không query DB trong vòng lặp; ghi hàng loạt dùng `*Range`
- [ ] Không tự gọi `SaveChangesAsync` (trừ `[SkipTransaction]` có lý do)
- [ ] Entity bị sửa được load bằng bản tracking
- [ ] Mọi entity con do method domain tự thêm đều được add tường minh (B7)
- [ ] SQL thô có điều kiện `tenant_id` và `is_deleted`

Kiểm chứng hiệu năng: `query.ToQueryString()` rồi chạy `EXPLAIN (ANALYZE, BUFFERS)` trong psql, với **trang sâu và dữ liệu thật**, không chỉ trang 1.
