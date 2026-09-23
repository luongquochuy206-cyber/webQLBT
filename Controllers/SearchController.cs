using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;

namespace QLBaiGuiXe.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var query = _context.LuotGuiXes
                .Include(x => x.MaVe)
                .Include(x => x.HoaDon)
                .OrderByDescending(x => x.ThoiGianVao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.BienSoXe.Contains(keyword) ||
                    (x.MaVe != null && x.MaVe.MaVeCode.Contains(keyword)));
            }

            return View(await query.ToListAsync());
        }
    }
}