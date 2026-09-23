using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;
    }
}