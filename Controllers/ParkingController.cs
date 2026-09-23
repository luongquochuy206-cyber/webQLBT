using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;
using QLBaiGuiXe.ViewModels;

namespace QLBaiGuiXe.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class ParkingController : Controller
    {
        private readonly AppDbContext _context;

        public ParkingController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetNhanVienId()
        {
            var claim = User.FindFirst("NhanVienId")?.Value;
            if (int.TryParse(claim, out int id)) return id;
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> CheckIn()
        {
            await LoadCheckInDataAsync();
            return View(new CheckInVM());
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(CheckInVM model)
        {
            await LoadCheckInDataAsync();

            if (model.LoaiGui == "Thang" && string.IsNullOrWhiteSpace(model.MaVeCode))
            {
                ModelState.AddModelError(nameof(model.MaVeCode), "Nhập mã vé tháng.");
            }

            if (!ModelState.IsValid) return View(model);

            var bienSo = model.BienSoXe.Trim().ToUpper();

            var dangTrongBai = await _context.LuotGuiXes
                .AnyAsync(x => x.BienSoXe == bienSo && x.TrangThai == "TrongBai");

            if (dangTrongBai)
            {
                ModelState.AddModelError("", "Xe này đang ở trong bãi.");
                return View(model);
            }

            bool laVeThang = false;
            int? dangKyVeThangId = null;
            MaVe? maVe;

            if (model.LoaiGui == "Thang")
            {
                var maVeCode = model.MaVeCode.Trim().ToUpper();
                maVe = await _context.MaVes
                    .FirstOrDefaultAsync(x =>
                        x.MaVeCode == maVeCode &&
                        x.LoaiMaVe == "Thang" &&
                        x.TrangThai == "Available");

                if (maVe == null)
                {
                    ModelState.AddModelError(nameof(model.MaVeCode), "Mã vé tháng không tồn tại hoặc đang được dùng.");
                    return View(model);
                }

                var today = DateTime.Today;
                var dk = await _context.DangKyVeThangs
                    .FirstOrDefaultAsync(x =>
                        x.MaVeId == maVe.Id &&
                        x.TrangThai &&
                        x.BienSoXe.ToUpper() == bienSo &&
                        x.LoaiXe == model.LoaiXe &&
                        x.NgayBatDau.Date <= today &&
                        x.NgayHetHan.Date >= today);

                if (dk == null)
                {
                    ModelState.AddModelError(nameof(model.MaVeCode), "Mã vé tháng không đúng xe đăng ký hoặc đã hết hạn.");
                    return View(model);
                }

                laVeThang = true;
                dangKyVeThangId = dk.Id;
            }
            else
            {
                maVe = await _context.MaVes
                    .Where(x => x.LoaiMaVe == "Luot" && x.TrangThai == "Available")
                    .OrderBy(x => x.MaVeCode)
                    .FirstOrDefaultAsync();

                if (maVe == null)
                {
                    ModelState.AddModelError("", "Không còn vé lượt trống.");
                    return View(model);
                }
            }

            maVe.TrangThai = "InUse";

            var luot = new LuotGuiXe
            {
                MaVeId = maVe.Id,
                BienSoXe = bienSo,
                LoaiXe = model.LoaiXe,
                ThoiGianVao = DateTime.Now,
                TrangThai = "TrongBai",
                LaVeThang = laVeThang,
                DangKyVeThangId = dangKyVeThangId,
                NhanVienVaoId = GetNhanVienId(),
                GhiChu = laVeThang ? "Vé tháng hợp lệ." : "Tự động cấp vé lượt."
            };

            _context.LuotGuiXes.Add(luot);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Xe vào thành công. Mã vé đang dùng: {maVe.MaVeCode}";
            return RedirectToAction(nameof(CheckIn));
        }

        [HttpGet]
        public IActionResult CheckOut()
        {
            return View(new CheckOutVM());
        }

        [HttpPost]
        public async Task<IActionResult> FindCheckOut(CheckOutVM model)
        {
            if (string.IsNullOrWhiteSpace(model.MaVeCode))
            {
                TempData["Error"] = "Nhập mã vé.";
                return View("CheckOut", new CheckOutVM());
            }

            var luot = await _context.LuotGuiXes
                .Include(x => x.MaVe)
                .Include(x => x.DangKyVeThang)
                .FirstOrDefaultAsync(x =>
                    x.MaVe!.MaVeCode == model.MaVeCode.Trim().ToUpper() &&
                    x.TrangThai == "TrongBai");

            if (luot == null)
            {
                TempData["Error"] = "Không tìm thấy xe đang ở trong bãi với mã vé này.";
                return View("CheckOut", new CheckOutVM());
            }

            var vm = new CheckOutVM
            {
                DaTimThay = true,
                LuotGuiXeId = luot.Id,
                MaVeCode = luot.MaVe!.MaVeCode,
                BienSoXe = luot.BienSoXe,
                LoaiXe = luot.LoaiXe,
                ThoiGianVao = luot.ThoiGianVao,
                ThoiGianRa = DateTime.Now,
                LaVeThang = luot.LaVeThang,
                SoTien = await TinhTienAsync(luot)
            };

            return View("CheckOut", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckOut(int luotGuiXeId)
        {
            var luot = await _context.LuotGuiXes
                .Include(x => x.MaVe)
                .Include(x => x.DangKyVeThang)
                .Include(x => x.HoaDon)
                .FirstOrDefaultAsync(x => x.Id == luotGuiXeId && x.TrangThai == "TrongBai");

            if (luot == null)
            {
                TempData["Error"] = "Không tìm thấy lượt gửi xe hợp lệ.";
                return RedirectToAction(nameof(CheckOut));
            }

            var soTien = await TinhTienAsync(luot);

            luot.ThoiGianRa = DateTime.Now;
            luot.TrangThai = "DaRa";
            luot.NhanVienRaId = GetNhanVienId();

            if (luot.MaVe != null)
                luot.MaVe.TrangThai = "Available";

            if (luot.HoaDon == null)
            {
                _context.HoaDons.Add(new HoaDon
                {
                    LuotGuiXeId = luot.Id,
                    SoTien = soTien,
                    ThoiGianThanhToan = DateTime.Now,
                    HinhThucThanhToan = "TienMat",
                    GhiChu = luot.LaVeThang ? "Xe tháng" : "Xe lượt"
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Xe ra thành công. Số tiền: {soTien:N0} VNĐ";
            return RedirectToAction(nameof(CheckOut));
        }

        [HttpGet]
        public async Task<IActionResult> History(DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.LuotGuiXes
                .Include(x => x.MaVe)
                .Include(x => x.HoaDon)
                .OrderByDescending(x => x.ThoiGianVao)
                .AsQueryable();

            if (tuNgay.HasValue)
                query = query.Where(x => x.ThoiGianVao.Date >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(x => x.ThoiGianVao.Date <= denNgay.Value.Date);

            return View(await query.ToListAsync());
        }

        private async Task LoadCheckInDataAsync()
        {
            ViewBag.LoaiXes = new List<string> { "XeMay", "OTo", "XeDap" };
            ViewBag.TongVeLuotTrongKho = await _context.MaVes.CountAsync(x => x.LoaiMaVe == "Luot" && x.TrangThai == "Available");
        }

        private async Task<decimal> TinhTienAsync(LuotGuiXe luot)
        {
            if (luot.LaVeThang)
                return 0;

            var bangGia = await _context.BangGias.FirstOrDefaultAsync(x => x.LoaiXe == luot.LoaiXe && x.DangApDung);
            if (bangGia == null) return 0;

            var gioGui = (DateTime.Now - luot.ThoiGianVao).TotalHours;
            var soBlock = Math.Max(1, (int)Math.Ceiling(gioGui / 12.0)); // mỗi 12 giờ tính 1 lượt
            return bangGia.GiaLuot * soBlock;
        }
    }
}
