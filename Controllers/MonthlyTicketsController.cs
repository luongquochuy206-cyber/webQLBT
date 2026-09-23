using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;
using QLBaiGuiXe.ViewModels;

namespace QLBaiGuiXe.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class MonthlyTicketsController : Controller
    {
        private readonly AppDbContext _context;

        public MonthlyTicketsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var query = _context.DangKyVeThangs
                .Include(x => x.MaVe)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.HoTenKhachHang.Contains(keyword) ||
                    x.BienSoXe.Contains(keyword) ||
                    x.CCCD.Contains(keyword) ||
                    (x.MaVe != null && x.MaVe.MaVeCode.Contains(keyword)));
            }

            return View(await query.OrderByDescending(x => x.Id).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadMaVeThangAsync();
            return View(new MonthlyTicketCreateVM
            {
                NgayBatDau = DateTime.Today,
                NgayHetHan = DateTime.Today.AddMonths(1)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(MonthlyTicketCreateVM model)
        {
            await LoadMaVeThangAsync();

            if (!ModelState.IsValid) return View(model);

            if (model.NgayHetHan < model.NgayBatDau)
            {
                ModelState.AddModelError("", "Ngày hết hạn phải lớn hơn hoặc bằng ngày bắt đầu.");
                return View(model);
            }

            bool trungBien = await _context.DangKyVeThangs
                .AnyAsync(x => x.BienSoXe == model.BienSoXe && x.TrangThai);

            if (trungBien)
            {
                ModelState.AddModelError("", "Biển số xe đã có vé tháng đang hoạt động.");
                return View(model);
            }

            var maVe = await _context.MaVes
                .FirstOrDefaultAsync(x => x.Id == model.MaVeId && x.LoaiMaVe == "Thang");

            if (maVe == null)
            {
                ModelState.AddModelError("", "Mã vé tháng không hợp lệ.");
                return View(model);
            }

            var dk = new DangKyVeThang
            {
                MaVeId = model.MaVeId,
                HoTenKhachHang = model.HoTenKhachHang,
                CCCD = model.CCCD,
                BienSoXe = model.BienSoXe.Trim().ToUpper(),
                LoaiXe = model.LoaiXe,
                NgayBatDau = model.NgayBatDau,
                NgayHetHan = model.NgayHetHan,
                SoTien = model.SoTien,
                TrangThai = true
            };

            _context.DangKyVeThangs.Add(dk);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký vé tháng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var dk = await _context.DangKyVeThangs.FindAsync(id);
            if (dk == null) return NotFound();

            dk.TrangThai = !dk.TrangThai;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật trạng thái vé tháng.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadMaVeThangAsync()
        {
            var daDangKyIds = await _context.DangKyVeThangs.Select(x => x.MaVeId).ToListAsync();

            var ds = await _context.MaVes
                .Where(x => x.LoaiMaVe == "Thang" && !daDangKyIds.Contains(x.Id))
                .OrderBy(x => x.MaVeCode)
                .ToListAsync();

            ViewBag.MaVeList = new SelectList(ds, "Id", "MaVeCode");
            ViewBag.LoaiXes = new List<string> { "XeMay", "OTo", "XeDap" };
        }
    }
}