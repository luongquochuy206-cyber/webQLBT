using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Models;

namespace QLBaiGuiXe.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>();

            if (!await db.TaiKhoans.AnyAsync())
            {
                var admin = new TaiKhoan
                {
                    TenDangNhap = "admin",
                    VaiTro = "Admin",
                    TrangThai = true
                };
                admin.MatKhauHash = hasher.HashPassword(admin, "admin123");

                var nv1 = new TaiKhoan
                {
                    TenDangNhap = "nv1",
                    VaiTro = "NhanVien",
                    TrangThai = true
                };
                nv1.MatKhauHash = hasher.HashPassword(nv1, "123456");

                db.TaiKhoans.AddRange(admin, nv1);
                await db.SaveChangesAsync();

                db.NhanViens.AddRange(
                    new NhanVien
                    {
                        HoTen = "Quản trị viên",
                        SoDienThoai = "0900000001",
                        Email = "admin@gmail.com",
                        TaiKhoanId = admin.Id
                    },
                    new NhanVien
                    {
                        HoTen = "Nhân viên 1",
                        SoDienThoai = "0900000002",
                        Email = "nv1@gmail.com",
                        TaiKhoanId = nv1.Id
                    }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.BangGias.AnyAsync())
            {
                db.BangGias.AddRange(
                    new BangGia { LoaiXe = "XeMay", GiaLuot = 5000, GiaThang = 100000, DangApDung = true },
                    new BangGia { LoaiXe = "OTo", GiaLuot = 30000, GiaThang = 800000, DangApDung = true },
                    new BangGia { LoaiXe = "XeDap", GiaLuot = 3000, GiaThang = 50000, DangApDung = true }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.MaVes.AnyAsync(x => x.LoaiMaVe == "Luot"))
            {
                var list = new List<MaVe>();
                for (int i = 1; i <= 300; i++)
                {
                    list.Add(new MaVe
                    {
                        MaVeCode = $"L{i:000}",
                        LoaiMaVe = "Luot",
                        TrangThai = "Available"
                    });
                }
                db.MaVes.AddRange(list);
                await db.SaveChangesAsync();
            }

            if (!await db.MaVes.AnyAsync(x => x.LoaiMaVe == "Thang"))
            {
                var list = new List<MaVe>();
                for (int i = 1; i <= 100; i++)
                {
                    list.Add(new MaVe
                    {
                        MaVeCode = $"T{i:000}",
                        LoaiMaVe = "Thang",
                        TrangThai = "Available"
                    });
                }
                db.MaVes.AddRange(list);
                await db.SaveChangesAsync();
            }

            if (!await db.ThietBis.AnyAsync())
            {
                var hvac = new ThietBi { MaThietBi = "HVAC-001", Ten = "Điều hòa trung tâm", KhuVuc = "Tầng 3 · Phòng máy", NhaCungCap = "Daikin Việt Nam", TrangThai = "Hoạt động", NgayLapDat = DateTime.Today.AddYears(-2) };
                var elevator = new ThietBi { MaThietBi = "ELV-002", Ten = "Thang máy A", KhuVuc = "Sảnh chính", NhaCungCap = "Mitsubishi Electric", TrangThai = "Cần sửa chữa", NgayLapDat = DateTime.Today.AddYears(-4) };
                var pump = new ThietBi { MaThietBi = "PMP-003", Ten = "Máy bơm nước PCCC", KhuVuc = "Tầng hầm B1", NhaCungCap = "Pentax", TrangThai = "Hoạt động", NgayLapDat = DateTime.Today.AddYears(-1) };
                var generator = new ThietBi { MaThietBi = "GEN-004", Ten = "Máy phát điện dự phòng", KhuVuc = "Khu kỹ thuật", NhaCungCap = "Cummins", TrangThai = "Hoạt động", NgayLapDat = DateTime.Today.AddYears(-3) };
                db.ThietBis.AddRange(hvac, elevator, pump, generator);
                await db.SaveChangesAsync();
                db.LichBaoTris.AddRange(
                    new LichBaoTri { ThietBiId = hvac.Id, NoiDung = "Vệ sinh lọc gió và kiểm tra gas", NgayDuKien = DateTime.Today.AddDays(2), NhanVien = "Nguyễn Văn Minh", ChiPhi = 850000 },
                    new LichBaoTri { ThietBiId = elevator.Id, NoiDung = "Kiểm tra cáp và hệ thống phanh", NgayDuKien = DateTime.Today.AddDays(5), NhanVien = "Trần Đức Long", ChiPhi = 2400000 },
                    new LichBaoTri { ThietBiId = pump.Id, NoiDung = "Chạy thử máy bơm định kỳ", NgayDuKien = DateTime.Today.AddDays(12), NhanVien = "Nguyễn Văn Minh", ChiPhi = 300000 });
                db.PhieuSuCos.AddRange(
                    new PhieuSuCo { ThietBiId = elevator.Id, TieuDe = "Thang máy phát tiếng ồn", MoTa = "Có tiếng kêu khi di chuyển lên tầng 4", MucDo = "Trung bình", TrangThai = "Đang xử lý", NguoiBao = "Lễ tân", NhanVienXuLy = "Trần Đức Long", NgayTao = DateTime.Now.AddHours(-4) },
                    new PhieuSuCo { ThietBiId = hvac.Id, TieuDe = "Điều hòa làm mát yếu", MoTa = "Nhiệt độ phòng máy cao hơn cài đặt", MucDo = "Thấp", TrangThai = "Mới tiếp nhận", NguoiBao = "Nguyễn An", NhanVienXuLy = "Chưa phân công", NgayTao = DateTime.Now.AddDays(-1) });
                await db.SaveChangesAsync();
            }
        }
    }
}
