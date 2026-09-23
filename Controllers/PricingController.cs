using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;

namespace QLBaiGuiXe.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PricingController : Controller
    {
        private readonly AppDbContext _context;

        public PricingController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BangGias.OrderBy(x => x.Id).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.BangGias.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BangGia model)
        {
            if (!ModelState.IsValid) return View(model);

            var item = await _context.BangGias.FindAsync(model.Id);
            if (item == null) return NotFound();

            item.LoaiXe = model.LoaiXe;
            item.GiaLuot = model.GiaLuot;
            item.GiaThang = model.GiaThang;
            item.DangApDung = model.DangApDung;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật bảng giá thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}