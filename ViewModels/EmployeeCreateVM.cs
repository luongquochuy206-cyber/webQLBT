using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.ViewModels
{
    public class EmployeeCreateVM
    {
        [Required(ErrorMessage = "Nhập họ tên")]
        public string HoTen { get; set; } = string.Empty;

        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chọn vai trò")]
        public string VaiTro { get; set; } = "NhanVien";
    }
}