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
            ViewBag.StudentCount = await _context.SinhViens.CountAsync();
            ViewBag.CourseCount = await _context.HocPhans.CountAsync();
            ViewBag.ClassCount = await _context.LopHocPhans.CountAsync();
            ViewBag.EnrollmentCount = await _context.DangKyHocPhans.CountAsync();
            ViewBag.RecentEnrollments = await _context.DangKyHocPhans.Include(x => x.SinhVien).Include(x => x.LopHocPhan!).ThenInclude(x => x.HocPhan).OrderByDescending(x => x.NgayDangKy).Take(6).ToListAsync();
            ViewBag.OpenSections = await _context.LopHocPhans.Include(x => x.HocPhan).Include(x => x.DangKys).OrderBy(x => x.Thu).ThenBy(x => x.TietBatDau).Take(5).ToListAsync();
            return View();
        }
    }
}
