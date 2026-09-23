using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models
{
    public class TaiKhoan
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        public string MatKhauHash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string VaiTro { get; set; } = "NhanVien";

        public bool TrangThai { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public NhanVien? NhanVien { get; set; }
    }
}