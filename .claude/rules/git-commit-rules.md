# Git: branch, commit, pull request

## Branch

- `main`: luôn build được, chỉ merge qua Pull Request.
- Branch làm việc: `{type}/{module}-{mo-ta-ngan}`, chữ thường, kebab-case, ví dụ `feat/inventory-warehouse-list`, `fix/accounting-voucher-total`, `chore/project-setup`.

## Commit message (commitlint kiểm tra tự động)

```
<type>(<scope>): <short message>

- change 1
- change 2
```

- **type**: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `build`, `ci`, `style`, `revert`, `hotfix`.
- **scope**: module hoặc phần hệ thống, kebab-case: `admin`, `inventory`, `accounting`, `projects`, `api`, `web`, `db`, `ci`, `auth`.
- **short message**: tiếng Anh, động từ ở đầu, ≤ 72 ký tự, không dấu chấm cuối. Ví dụ: `feat(inventory): add warehouse list api`.
- Không dùng message chung chung: `update code`, `fix bug`, `done`.
- Không nhắc tới AI/công cụ sinh code trong commit message.
- Mỗi commit một mục đích. Tách commit backend/frontend khi thay đổi lớn.

## Trước khi commit / push

- Pre-commit hook tự chạy lint + format cho file đã stage (FE: ESLint, Prettier; BE: `dotnet format`).
- Trước khi push: build và test phần mình sửa phải xanh. Không bỏ qua test lỗi để qua CI.
- Không commit: `.env*` (trừ `.env.example`), `appsettings.*.local.json`, mật khẩu, token, dữ liệu khách hàng thật.

## Pull Request

- Điền đủ mẫu `.github/pull_request_template.md`: mô tả, link đặc tả chức năng, ảnh chụp màn hình (nếu có UI), migration (nếu có).
- CI phải xanh và có ít nhất một người review trước khi merge.
- Ưu tiên **Squash and merge** để lịch sử `main` gọn.
