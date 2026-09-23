using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.ViewModels
{
    public class EmployeeEditVM
    {
        public int Id { get; set; }
        public int TaiKhoanId { get; set; }

        [Required(ErrorMessage = "Nhập họ tên")]
        public string HoTen { get; set; } = string.Empty;

        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Chọn vai trò")]
        public string VaiTro { get; set; } = "NhanVien";

        public bool TrangThai { get; set; } = true;
    }
}