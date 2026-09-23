using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.ViewModels
{
    public class TicketCodeCreateVM
    {
        [Required]
        public string Prefix { get; set; } = "L";

        [Range(1, 10000)]
        public int StartNumber { get; set; } = 1;

        [Range(1, 1000)]
        public int SoLuong { get; set; } = 1;

        [Required]
        public string LoaiMaVe { get; set; } = "Luot";
    }
}