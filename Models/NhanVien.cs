using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models
{
    public class NhanVien
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public int TaiKhoanId { get; set; }
        public TaiKhoan? TaiKhoan { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<LuotGuiXe>? LuotGuiXeVao { get; set; }
        public ICollection<LuotGuiXe>? LuotGuiXeRa { get; set; }
    }
}