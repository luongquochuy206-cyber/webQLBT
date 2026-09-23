using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models;

public class ThietBi
{
    public int Id { get; set; }
    [Required, StringLength(30)] public string MaThietBi { get; set; } = "";
    [Required, StringLength(120)] public string Ten { get; set; } = "";
    [StringLength(80)] public string KhuVuc { get; set; } = "";
    [StringLength(100)] public string NhaCungCap { get; set; } = "";
    [StringLength(30)] public string TrangThai { get; set; } = "Hoạt động";
    public DateTime NgayLapDat { get; set; } = DateTime.Today;
    public ICollection<LichBaoTri> LichBaoTris { get; set; } = new List<LichBaoTri>();
    public ICollection<PhieuSuCo> PhieuSuCos { get; set; } = new List<PhieuSuCo>();
}

public class LichBaoTri
{
    public int Id { get; set; }
    public int ThietBiId { get; set; }
    public ThietBi? ThietBi { get; set; }
    [Required, StringLength(160)] public string NoiDung { get; set; } = "";
    public DateTime NgayDuKien { get; set; }
    [StringLength(80)] public string NhanVien { get; set; } = "Kỹ thuật viên";
    [StringLength(30)] public string TrangThai { get; set; } = "Đã lên lịch";
    public decimal ChiPhi { get; set; }
}

public class PhieuSuCo
{
    public int Id { get; set; }
    public int ThietBiId { get; set; }
    public ThietBi? ThietBi { get; set; }
    [Required, StringLength(160)] public string TieuDe { get; set; } = "";
    [Required, StringLength(1200)] public string MoTa { get; set; } = "";
    [StringLength(20)] public string MucDo { get; set; } = "Trung bình";
    [StringLength(30)] public string TrangThai { get; set; } = "Mới tiếp nhận";
    [StringLength(80)] public string NguoiBao { get; set; } = "Nhân viên tiếp nhận";
    [StringLength(80)] public string NhanVienXuLy { get; set; } = "Chưa phân công";
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public decimal ChiPhi { get; set; }
}
