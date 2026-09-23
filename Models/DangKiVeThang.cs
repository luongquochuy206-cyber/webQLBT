using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models
{
    public class DangKyVeThang
    {
        public int Id { get; set; }

        [Required]
        public int MaVeId { get; set; }
        public MaVe? MaVe { get; set; }

        [Required, StringLength(100)]
        public string HoTenKhachHang { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string CCCD { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string BienSoXe { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string LoaiXe { get; set; } = string.Empty;

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayHetHan { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SoTien { get; set; }

        public bool TrangThai { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}