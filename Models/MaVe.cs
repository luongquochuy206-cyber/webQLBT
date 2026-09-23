using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models
{
    public class MaVe
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string MaVeCode { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string LoaiMaVe { get; set; } = "Luot"; // Luot / Thang

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "Available"; // Available / InUse / Inactive

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? GhiChu { get; set; }
    }
}