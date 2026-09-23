using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;

namespace QLBaiGuiXe.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.DeviceCount = await _context.ThietBis.CountAsync();
            ViewBag.OpenTickets = await _context.PhieuSuCos.CountAsync(x => x.TrangThai != "Hoàn tất");
            ViewBag.DueSoon = await _context.LichBaoTris.CountAsync(x => x.TrangThai == "Đã lên lịch" && x.NgayDuKien <= DateTime.Today.AddDays(7));
            ViewBag.BrokenDevices = await _context.ThietBis.CountAsync(x => x.TrangThai == "Cần sửa chữa");
            ViewBag.RecentTickets = await _context.PhieuSuCos.Include(x => x.ThietBi).OrderByDescending(x => x.NgayTao).Take(5).ToListAsync();
            ViewBag.Upcoming = await _context.LichBaoTris.Include(x => x.ThietBi).Where(x => x.TrangThai == "Đã lên lịch").OrderBy(x => x.NgayDuKien).Take(5).ToListAsync();
            return View();
        }
    }
}
