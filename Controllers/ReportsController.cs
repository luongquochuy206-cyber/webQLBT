using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;

namespace QLBaiGuiXe.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            if (!tuNgay.HasValue) tuNgay = DateTime.Today;
            if (!denNgay.HasValue) denNgay = DateTime.Today;

            var from = tuNgay.Value.Date;
            var to = denNgay.Value.Date.AddDays(1).AddSeconds(-1);

            var luot = await _context.LuotGuiXes
                .Where(x => x.ThoiGianVao >= from && x.ThoiGianVao <= to)
                .ToListAsync();

            var hoadons = await _context.HoaDons
                .Where(x => x.ThoiGianThanhToan >= from && x.ThoiGianThanhToan <= to)
                .Include(x => x.LuotGuiXe)
                .ToListAsync();

            ViewBag.TuNgay = tuNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay.Value.ToString("yyyy-MM-dd");
            ViewBag.TongXeGui = luot.Count;
            ViewBag.SoXeDaRa = luot.Count(x => x.TrangThai == "DaRa");
            ViewBag.SoXeConTrongBai = luot.Count(x => x.TrangThai == "TrongBai");
            ViewBag.DoanhThu = hoadons.Sum(x => x.SoTien);

            ViewBag.TheoLoaiXe = hoadons
                .GroupBy(x => x.LuotGuiXe!.LoaiXe)
                .Select(g => new
                {
                    LoaiXe = g.Key,
                    SoLuong = g.Count(),
                    DoanhThu = g.Sum(x => x.SoTien)
                })
                .ToList();

            return View();
        }
    }
}