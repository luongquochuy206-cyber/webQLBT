using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.ViewModels
{
    public class CheckInVM
    {
        [Required(ErrorMessage = "Nhập biển số xe")]
        public string BienSoXe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chọn loại xe")]
        public string LoaiXe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chọn loại gửi")]
        public string LoaiGui { get; set; } = "Luot";

        public string MaVeCode { get; set; } = string.Empty;
    }
}
