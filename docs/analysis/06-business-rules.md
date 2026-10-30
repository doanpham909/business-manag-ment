# 06 — Business Rules quan trọng

Chỉ liệt kê quy tắc ảnh hưởng thiết kế dữ liệu hoặc nhiều chức năng. Quy tắc chi tiết từng màn hình nằm trong đặc tả chức năng sau này. Mã `BR-{MODULE}-xx` giữ nguyên khi chuyển sang `docs/features/`.

## Chung (áp mọi phân hệ)

| Mã | Quy tắc | |
|---|---|---|
| BR-GEN-01 | Mọi dữ liệu nghiệp vụ thuộc đúng một tenant; không có API nào trả dữ liệu tenant khác | [C] |
| BR-GEN-01b | Dữ liệu nghiệp vụ thuộc đúng một công ty; người dùng chỉ thấy dữ liệu của công ty đang chọn. Công ty lấy từ token, không nhận từ client (DEC-003, DEC-005) | [C] |
| BR-GEN-02 | Mã chứng từ unique trong công ty; mã danh mục dùng chung unique trong tenant (DEC-008) | [C] |
| BR-GEN-03 | Không xóa cứng bản ghi đã được tham chiếu; dùng "Ngừng sử dụng" (danh mục) hoặc "Hủy" (chứng từ) | [C] |
| BR-GEN-04 | Bản ghi "ngừng sử dụng" không chọn được cho giao dịch mới nhưng vẫn hiển thị đúng trên chứng từ cũ | [C] |
| BR-GEN-05 | Chứng từ lưu snapshot thông tin ảnh hưởng ý nghĩa của nó; sửa danh mục về sau không đổi chứng từ cũ | [P] |
| BR-GEN-06 | Chứng từ đã Approved/Confirmed/Locked không sửa trực tiếp; thay đổi bằng chứng từ điều chỉnh hoặc đảo, có duyệt | [C] |
| BR-GEN-07 | Dữ liệu thuộc kỳ đã khóa không ghi/sửa được; mở khóa kỳ cần quyền `lock` và ghi audit kèm lý do | [C] |
| BR-GEN-08 | Hai người cùng sửa: người lưu sau nhận lỗi xung đột (409) và phải tải lại; không ghi đè im lặng | [P] |
| BR-GEN-09 | Thao tác tạo chứng từ / xác nhận / chi tiền / chốt kỳ có idempotency key; gửi 2 lần chỉ ghi nhận 1 lần | [P] |
| BR-GEN-10 | Số liệu tổng hợp (tồn, công nợ còn lại, đã cấp phát, ngân sách còn lại) tính từ sổ giao dịch, không nhập tay | [P] |
| BR-GEN-11 | Import Excel: kiểm tra toàn bộ file trước; có dòng lỗi thì **người dùng chọn** ghi các dòng hợp lệ hoặc huỷ; tải được file chi tiết lỗi theo dòng về máy | [C] DEC-021 |
| BR-GEN-12 | Export và báo cáo tuân theo đúng quyền và data scope của màn hình tương ứng | [C] |
| BR-GEN-13 | Tiền lưu `numeric(18,2)`, số lượng `numeric(18,4)` [P]; đơn vị tiền mặc định VND; làm tròn theo `Q-FIN-10` | [P] |
| BR-GEN-14 | Ngày giờ lưu UTC, hiển thị theo múi giờ tenant (mặc định Asia/Ho_Chi_Minh) | [P] |

## ADM

| Mã | Quy tắc | |
|---|---|---|
| BR-ADM-01 | Username unique toàn nền tảng, vì đăng nhập không nhập mã công ty (DEC-005) | [C] |
| BR-ADM-01b | Sau đăng nhập user chọn công ty trong danh sách được phép; chỉ một công ty thì vào thẳng; không có công ty nào thì không vào được và nhận thông báo liên hệ quản trị. Đổi công ty thì server cấp token mới sau khi kiểm tra quyền | [C]/[P] |
| BR-ADM-02 | Không gỡ quyền/khóa người quản trị cuối cùng của tenant | [C] |
| BR-ADM-03 | Không tự nâng quyền/scope vượt quyền đang có | [C] |
| BR-ADM-04 | Mỗi loại chứng từ có tối đa một workflow đang hiệu lực (theo điều kiện); sửa workflow tạo phiên bản mới, instance đang chạy giữ phiên bản cũ | [P] |
| BR-ADM-05 | Không xóa vai trò còn user được gán | [P] |
| BR-ADM-06 | Audit log chỉ ghi thêm; không người dùng nào sửa/xóa được qua ứng dụng | [C] |
| BR-ADM-07 | Mã danh mục unique theo `(tenant, catalog_type)`; danh mục đang dùng chỉ ngừng sử dụng, không xóa | [C] |

## HRM

| Mã | Quy tắc | |
|---|---|---|
| BR-HRM-01 | Đơn vị không làm cha của chính nó hoặc của tổ tiên nó (không vòng lặp) | [C] |
| BR-HRM-02 | Không ngừng/xóa đơn vị còn nhân viên đang công tác hoặc còn đơn vị con hoạt động | [C] |
| BR-HRM-03 | Đổi đơn vị cha hiển thị trước ảnh hưởng: số nhân viên, số user có scope `department-tree` bị đổi | [P] |
| BR-HRM-04 | Mã nhân viên unique trong tenant | [C] |
| BR-HRM-05 | Mỗi nhân viên có đúng một trạng thái và một phân công chính hiện hành tại một thời điểm | [C]/[P] |
| BR-HRM-06 | Điều chuyển lưu bằng bản ghi phân công mới có ngày hiệu lực; không sửa đè phân công cũ | [P] |
| BR-HRM-07 | Không xóa hồ sơ nhân viên đã có chấm công, bảng lương, tạm ứng hoặc chứng từ kế toán | [C] |

## PAY

| Mã | Quy tắc | |
|---|---|---|
| BR-PAY-01 | Ca qua ngày tính công cho ngày bắt đầu ca **[A]** | [U] `Q-PAY-02` |
| BR-PAY-02 | Dữ liệu vào/ra thô không tự coi là công hợp lệ; công chỉ được tính sau quy tắc ca + điều chỉnh đã duyệt | [C] |
| BR-PAY-03 | Mỗi nhân viên – mỗi ngày tối đa một giá trị công cho mỗi loại công; không tính trùng khi có nhiều bản ghi vào/ra | [C] |
| BR-PAY-04 | Điều chỉnh chấm công bắt buộc có lý do; lưu giá trị cũ/mới | [C] |
| BR-PAY-05 | Một kỳ bảng công / kỳ lương duy nhất cho cùng phạm vi (pháp nhân/đơn vị) và tháng | [C] |
| BR-PAY-06 | Tính lại bảng lương tạo lần tính (`run_no`) mới; chỉ lần tính được duyệt mới được khóa và chuyển FIN | [P] |
| BR-PAY-07 | Bảng lương chỉ lấy từ bảng công đã Approved/Locked của cùng kỳ | [C] |
| BR-PAY-08 | Thực lĩnh = Tổng thu nhập − Tổng khấu trừ. Thành phần và công thức là cấu hình được duyệt, không viết cứng trong code | [C] |
| BR-PAY-09 | Phiếu lương chỉ tạo từ bảng lương đã Locked; nhân viên chỉ xem phiếu của mình | [C] |
| BR-PAY-10 | Điều chỉnh sau khi khóa đi vào kỳ sau dưới dạng khoản truy lĩnh/truy thu **[A]**, không mở lại kỳ cũ | [U] `Q-PAY-09` |

## FIN

| Mã | Quy tắc | |
|---|---|---|
| BR-FIN-01 | Đề nghị thanh toán có hai trục trạng thái độc lập: duyệt và chi tiền; Approved ≠ đã chi | [C] |
| BR-FIN-02 | Tổng các lần chi của một đề nghị ≤ số tiền được duyệt | [C] |
| BR-FIN-03 | Ngân sách đã khóa chỉ thay đổi bằng phiên bản điều chỉnh có duyệt; giữ lịch sử phiên bản | [C] |
| BR-FIN-04 | Vượt ngân sách khi tạo đề nghị/đơn mua: chặn hay cảnh báo? | [U] `Q-FIN-11` |
| BR-FIN-05 | Bút toán cân: Σ Nợ = Σ Có trên mỗi chứng từ kế toán | [C] |
| BR-FIN-06 | Một nghiệp vụ nguồn chỉ sinh một bộ bút toán hiệu lực; sửa nguồn → bút toán đảo + bút toán mới | [C] |
| BR-FIN-07 | Không ghi nhận/sửa bút toán trong kỳ kế toán đã khóa | [C] |
| BR-FIN-08 | Doanh thu không tự suy ra từ sản lượng hay tiền đã thu | [C] |
| BR-FIN-09 | Tách rõ: giá trị mua, giá trị nhập kho, giá trị xuất kho, tiêu hao, chi phí ghi nhận — không đồng nhất | [C] |
| BR-FIN-10 | Tạm ứng: số hoàn ứng + số quyết toán ≤ số đã chi tạm ứng; tạm ứng quá hạn hoàn ứng được cảnh báo | [C]/[P] |
| BR-FIN-11 | Tuổi nợ tính từ hạn thanh toán (không phải ngày chứng từ) **[A]** | [U] `Q-FIN-12` |
| BR-FIN-12 | Báo cáo tài chính ghi rõ: nguồn dữ liệu, đơn vị tiền, kỳ, phương pháp tính, trạng thái chốt | [C] |

## WH

| Mã | Quy tắc | |
|---|---|---|
| BR-WH-01 | Chỉ phiếu Confirmed mới ghi `StockLedgerEntry`; phiếu nháp không đổi tồn | [C] |
| BR-WH-02 | Tồn cuối = Tồn đầu + Nhập hợp lệ − Xuất hợp lệ ± Điều chỉnh hợp lệ | [C] |
| BR-WH-03 | Tồn khả dụng = Tồn thực − Giữ chỗ còn hiệu lực | [C] (giữ chỗ khi nào: `Q-WH-03`) |
| BR-WH-04 | Không xuất vượt tồn khả dụng; ngoại lệ (tồn âm) chỉ khi tenant bật cấu hình **[A]** | [U] `Q-WH-02` |
| BR-WH-05 | Một yêu cầu vật tư cấp phát nhiều lần; Σ đã cấp ≤ Σ được duyệt | [C] |
| BR-WH-06 | Không giới hạn số nhận theo đơn mua; dòng nhận khác số đặt (vượt hoặc thiếu) đánh dấu đỏ; nhận vượt thì cảnh báo | [C] DEC-018, DEC-019 |
| BR-WH-07 | Chuyển kho: kho nguồn ≠ kho đích; hàng ghi "đi đường" từ lúc gửi tới lúc nhận; không cộng trùng tồn | [C] |
| BR-WH-08 | Kho ngừng hoạt động không nhận giao dịch mới; không xóa kho đã có giao dịch | [C] |
| BR-WH-09 | Kiểm kê không tự điều chỉnh tồn; điều chỉnh chỉ ghi khi đợt kiểm kê được duyệt, theo mốc cut-off | [C] |
| BR-WH-10 | Giao dịch sau mốc cut-off kiểm kê không làm sai số hệ thống của đợt kiểm kê | [P] |
| BR-WH-11 | Đơn vị quy đổi: mọi số lượng lưu sổ theo đơn vị cơ sở của vật tư | [P] |
| BR-WH-12 | Đảo phiếu đã xác nhận bằng phiếu ngược; không đảo được nếu làm tồn tại thời điểm đó âm | [P] |

## PRJ

| Mã | Quy tắc | |
|---|---|---|
| BR-PRJ-01 | Mã dự án unique trong tenant | [C] |
| BR-PRJ-02 | Dự án gắn với một công ty quản lý hợp lệ (pháp nhân, hoặc chính tenant nếu không có pháp nhân) | [C] |
| BR-PRJ-03 | Xoá được dự án chưa có chứng từ và chưa gán phạm vi; dự án đã có chứng từ không xoá được | [C] DEC-020 |
| BR-PRJ-04 | Dự án Hoàn thành hoặc Đóng không chọn được khi lập chứng từ mới, không ngoại lệ | [C] DEC-018, DEC-019 |
| BR-PRJ-05 | Sửa thông tin dự án không thay đổi chứng từ đã ghi nhận (snapshot BR-GEN-05) | [C] |
| BR-PRJ-06 | Ngày kết thúc dự kiến ≥ ngày bắt đầu | [P] |
| BR-PRJ-07 | Sao chép dự án: mang toàn bộ thông tin dự án trừ mã (mã mới bắt buộc), trạng thái Đang thực hiện, không sao chép dữ liệu ở phân hệ khác | [C] DEC-021 |
| BR-PRJ-08 | Dự án có thể có dự án con / gói thầu (dự án cha cùng công ty); không xoá dự án cha còn dự án con | [C] DEC-021 |
