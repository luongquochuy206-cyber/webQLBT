using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models
{
    public class LuotGuiXe
    {
        public int Id { get; set; }

        [Required]
        public int MaVeId { get; set; }
        public MaVe? MaVe { get; set; }

        [Required, StringLength(20)]
        public string BienSoXe { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string LoaiXe { get; set; } = string.Empty;

        public DateTime ThoiGianVao { get; set; } = DateTime.Now;

        public DateTime? ThoiGianRa { get; set; }

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "TrongBai"; // TrongBai / DaRa

        public bool LaVeThang { get; set; } = false;

        public int? DangKyVeThangId { get; set; }
        public DangKyVeThang? DangKyVeThang { get; set; }

        public int? NhanVienVaoId { get; set; }
        public NhanVien? NhanVienVao { get; set; }

        public int? NhanVienRaId { get; set; }
        public NhanVien? NhanVienRa { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public HoaDon? HoaDon { get; set; }
    }
}