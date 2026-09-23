using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models
{
    public class BangGia
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string LoaiXe { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal GiaLuot { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GiaThang { get; set; }

        public bool DangApDung { get; set; } = true;
    }
}