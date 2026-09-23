using System.ComponentModel.DataAnnotations;

namespace QLBaiGuiXe.Models;

public class SinhVien
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaSinhVien { get; set; } = "";
    [Required, StringLength(100)] public string HoTen { get; set; } = "";
    [EmailAddress, StringLength(120)] public string Email { get; set; } = "";
    [StringLength(80)] public string Nganh { get; set; } = "";
    [StringLength(30)] public string LopHanhChinh { get; set; } = "";
    public int Khoa { get; set; }
    public ICollection<DangKyHocPhan> DangKys { get; set; } = new List<DangKyHocPhan>();
}

public class HocPhan
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaHocPhan { get; set; } = "";
    [Required, StringLength(120)] public string TenHocPhan { get; set; } = "";
    [Range(1, 10)] public int TinChi { get; set; }
    [StringLength(20)] public string? MaTienQuyet { get; set; }
    [StringLength(100)] public string KhoaQuanLy { get; set; } = "CNTT";
}

public class LopHocPhan
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string MaLop { get; set; } = "";
    public int HocPhanId { get; set; }
    public HocPhan? HocPhan { get; set; }
    [StringLength(100)] public string GiangVien { get; set; } = "";
    [StringLength(80)] public string Phong { get; set; } = "";
    [Range(2, 7)] public int Thu { get; set; }
    [Range(1, 12)] public int TietBatDau { get; set; }
    [Range(1, 12)] public int SoTiet { get; set; } = 3;
    public int SiSoToiDa { get; set; } = 40;
    [StringLength(20)] public string HocKy { get; set; } = "2026_2027_1";
    public ICollection<DangKyHocPhan> DangKys { get; set; } = new List<DangKyHocPhan>();
}

public class DangKyHocPhan
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }
    public int LopHocPhanId { get; set; }
    public LopHocPhan? LopHocPhan { get; set; }
    public DateTime NgayDangKy { get; set; } = DateTime.Now;
    [StringLength(20)] public string TrangThai { get; set; } = "Đang học";
    public double? DiemQuaTrinh { get; set; }
    public double? DiemThi { get; set; }
    public double? DiemTongKet => DiemQuaTrinh.HasValue && DiemThi.HasValue ? Math.Round(DiemQuaTrinh.Value * .4 + DiemThi.Value * .6, 1) : null;
}
