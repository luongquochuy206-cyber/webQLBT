# UniFlow — Hệ thống quản lý đào tạo tích hợp AI

Website ASP.NET Core MVC (.NET 8), giao diện tiếng Việt, cơ sở dữ liệu SQL Server LocalDB. Có thể mở trực tiếp bằng Visual Studio 2022.

## Chạy bằng Visual Studio 2022

1. Cài workload **ASP.NET and web development** và .NET 8 SDK.
2. Mở `QLBaiGuiXe.sln` trong thư mục `QLBaiGuiXe-master`.
3. Chọn `QLBaiGuiXe` làm Startup Project, chọn profile `https` hoặc `http`, nhấn **F5**.
4. Ứng dụng tạo database `UniFlowAI` trên `(localdb)\MSSQLLocalDB` và nạp dữ liệu mẫu ở lần chạy đầu.

Tài khoản demo:

- Phòng đào tạo: `admin` / `admin123`
- Sinh viên: `DTC245200050` / `123456`

## Chức năng

- Dashboard thống kê sinh viên, học phần, lớp mở và lượt đăng ký.
- Tra cứu và thêm hồ sơ sinh viên; danh mục học phần, tiên quyết và lịch học.
- Đăng ký học phần có kiểm tra trùng lịch, tiên quyết, số chỗ và đăng ký lặp.
- Bảng điểm cá nhân, tính điểm tổng kết theo trọng số 40% quá trình và 60% thi.
- Trợ lý học vụ dùng dữ liệu chương trình đào tạo; chạy demo cục bộ khi chưa cấu hình AI.
- Có thể bật OpenAI API tương thích Chat Completions bằng cấu hình máy chủ.

## Cấu hình AI tùy chọn

Đặt các biến môi trường trước khi chạy. Không lưu API key vào mã nguồn hoặc `appsettings.json`.

```powershell
$env:AI__ApiKey = "your-api-key"
$env:AI__Model = "gpt-4o-mini"
dotnet run --project .\QLBaiGuiXe.csproj
```

Có thể đặt `AI__Endpoint` nếu dùng endpoint tương thích OpenAI khác. Khi chưa có key, trợ lý phản hồi bằng logic demo cục bộ. Khi có key, ứng dụng chỉ gửi câu hỏi và ngữ cảnh học phần liên quan sau khi người dùng chủ động gửi câu hỏi; điểm cá nhân chỉ được thêm vào ngữ cảnh khi câu hỏi liên quan điểm/kết quả học tập. Dịch vụ AI được yêu cầu không tự đặt quy chế và nhắc người dùng đối chiếu với phòng đào tạo.

## Cấu hình dữ liệu

Connection string nằm trong `appsettings.json` (`ConnectionStrings:DefaultConnection`), có thể ghi đè bằng `ConnectionStrings__DefaultConnection`. EF Core `EnsureCreated` tạo schema cho database mới. Nếu thay đổi schema trong database đã có dữ liệu, hãy tạo và áp dụng migration EF Core.

## Cấu trúc dữ liệu đào tạo

- `SinhVien`: mã sinh viên duy nhất, hồ sơ, ngành và lớp hành chính.
- `HocPhan`: mã học phần duy nhất, tín chỉ và mã học phần tiên quyết.
- `LopHocPhan`: lịch, phòng, giảng viên, sĩ số và khóa ngoại đến học phần.
- `DangKyHocPhan`: liên kết sinh viên/lớp, trạng thái và điểm; có ràng buộc duy nhất cho cặp sinh viên/lớp.

Ứng dụng hiện là bản demo học phần: dữ liệu tài khoản quản trị kế thừa mô hình mẫu; các nghiệp vụ chấm điểm và quản lý giảng viên có thể mở rộng tiếp theo.
