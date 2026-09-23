using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.ViewModels
{
    public class MonthlyTicketCreateVM
    {
        [Required]
        public int MaVeId { get; set; }

        [Required]
        public string HoTenKhachHang { get; set; } = string.Empty;

        [Required]
        public string CCCD { get; set; } = string.Empty;

        [Required]
        public string BienSoXe { get; set; } = string.Empty;

        [Required]
        public string LoaiXe { get; set; } = string.Empty;

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayHetHan { get; set; }

        [Required]
        public decimal SoTien { get; set; }
    }
}