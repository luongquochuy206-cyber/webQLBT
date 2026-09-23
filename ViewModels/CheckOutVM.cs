using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.ViewModels
{
    public class CheckOutVM
    {
        [Required(ErrorMessage = "Nhập mã vé")]
        public string MaVeCode { get; set; } = string.Empty;

        public int LuotGuiXeId { get; set; }
        public string BienSoXe { get; set; } = string.Empty;
        public string LoaiXe { get; set; } = string.Empty;
        public DateTime ThoiGianVao { get; set; }
        public DateTime? ThoiGianRa { get; set; }
        public decimal SoTien { get; set; }
        public bool LaVeThang { get; set; }
        public bool DaTimThay { get; set; }
    }
}