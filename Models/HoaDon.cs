using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models
{
    public class HoaDon
    {
        public int Id { get; set; }

        public int LuotGuiXeId { get; set; }
        public LuotGuiXe? LuotGuiXe { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SoTien { get; set; }

        public DateTime ThoiGianThanhToan { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? HinhThucThanhToan { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }
    }
}