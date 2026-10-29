# Kiểm thử

## Phải có test cho

| Tầng | Công cụ | Test gì |
|---|---|---|
| Domain (BE) | xUnit | Factory method, business rule (`IsBroken`), chuyển trạng thái |
| Application (BE) | xUnit | Validator (input hợp lệ/không hợp lệ), handler có logic rẽ nhánh |
| Kiến trúc (BE) | NetArchTest | Chiều phụ thuộc giữa các layer — đã có sẵn, không xóa/nới lỏng |
| FE | Vitest + Testing Library | Hàm xử lý dữ liệu (`utils`), hook có logic, form validation (Yup schema), component có điều kiện hiển thị |

Không viết test cho code chỉ chuyển tiếp (controller mỏng, component chỉ render tĩnh).

## Nguyên tắc

- Test đặt tên theo hành vi: `Create_WhenCodeIsEmpty_ThrowsBusinessRuleException` / `it('hiển thị lỗi khi API trả về 500')`.
- Mỗi test độc lập, không phụ thuộc thứ tự chạy, không gọi mạng/DB thật trong unit test.
- Không dùng thư viện có license thương mại (FluentAssertions v8+, Moq phiên bản có SponsorLink). Dùng `Assert` của xUnit hoặc Shouldly; mock bằng NSubstitute khi cần.
- Sửa bug: viết test tái hiện lỗi trước, rồi mới sửa.
- **Không** xóa, skip hoặc sửa kỳ vọng của test chỉ để CI xanh.

## Lệnh chạy

```bash
# Backend (từ apps/api)
dotnet build Bms.slnx -warnaserror
dotnet test Bms.slnx

# Frontend (từ apps/web)
npm run lint
npm run typecheck
npm run test:run
npm run build
```

CI (`.github/workflows/ci.yml`) chạy đúng các lệnh này. Máy local xanh thì CI phải xanh.

## Kiểm thử thủ công trước PR

Chạy API + web local, đi qua từng tiêu chí chấp nhận (`AC-xx`) của đặc tả, bao gồm: người dùng không có quyền, dữ liệu của tenant khác không hiện ra, dữ liệu rỗng, lỗi mạng.
