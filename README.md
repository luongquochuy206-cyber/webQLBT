# MaintainIQ — Quản lý bảo trì tòa nhà

Ứng dụng web ASP.NET Core MVC (.NET 8) cho đề tài AIA331. Giao diện và nội dung bằng tiếng Việt; dữ liệu lưu bằng SQL Server LocalDB.

## Mở bằng Visual Studio 2022

1. Cài workload **ASP.NET and web development** và .NET 8 SDK.
2. Mở `QLBaiGuiXe.sln`.
3. Đặt `QLBaiGuiXe` làm Startup Project, chọn profile `https` hoặc `http`, rồi nhấn **F5**.
4. Nếu chưa có database, ứng dụng tạo database `MaintainIQ` trên `(localdb)\MSSQLLocalDB` và nạp dữ liệu demo khi khởi động lần đầu.

Có thể chạy bằng terminal tại thư mục chứa file `.csproj`:

```powershell
dotnet restore
dotnet run
```

Mở URL được in trong terminal. Tài khoản demo: `admin` / `admin123`; nhân viên: `nv1` / `123456`.

## Chức năng

- Đăng nhập cookie, tài khoản và vai trò kế thừa từ ứng dụng mẫu.
- Dashboard tổng hợp thiết bị, sự cố đang mở, lịch bảo trì sắp tới và danh sách gần đây.
- CRUD thiết bị, tìm theo mã/tên/khu vực; ngăn xóa thiết bị đã có lịch sử.
- Tạo lịch bảo trì, xem lịch và đánh dấu hoàn tất.
- Tiếp nhận phiếu sự cố, gợi ý mức độ sơ bộ theo từ khóa, tìm kiếm và cập nhật nhân viên/trạng thái.
- Trợ lý demo tổng hợp dữ liệu thực tế từ thiết bị, lịch bảo trì, phiếu sự cố. Trợ lý hiện chạy bằng quy tắc cục bộ, chưa gọi API AI ngoài.

## Cấu hình database

Connection string ở `appsettings.json`, có thể ghi đè bằng biến môi trường `ConnectionStrings__DefaultConnection`. Mặc định cần SQL Server LocalDB (được cài cùng một số cấu hình Visual Studio). `EnsureCreated` tạo schema cho database mới; khi thay đổi schema sau này, hãy chuyển sang EF Core migrations trước khi giữ lại database cũ.

## Cấu trúc dữ liệu bảo trì

- `ThietBi`: mã duy nhất, thông tin thiết bị, khu vực, nhà cung cấp, trạng thái.
- `LichBaoTri`: lịch và chi phí, khóa ngoại tới `ThietBi`.
- `PhieuSuCo`: mô tả, mức độ, trạng thái và người xử lý, khóa ngoại tới `ThietBi`.

## Giới hạn demo

Phân loại mức độ hiện là gợi ý từ khóa, không thay thế đánh giá của kỹ thuật viên. Muốn gọi OpenAI/Gemini, hãy cấu hình API key ở phía máy chủ và bổ sung kiểm soát dữ liệu gửi đi, timeout, giới hạn nội dung và xử lý rate limit.
