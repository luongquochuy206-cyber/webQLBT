using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;
using QLBaiGuiXe.ViewModels;

namespace QLBaiGuiXe.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TicketCodesController : Controller
    {
        private readonly AppDbContext _context;

        public TicketCodesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var query = _context.MaVes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.MaVeCode.Contains(keyword) || x.LoaiMaVe.Contains(keyword));
            }

            return View(await query.OrderBy(x => x.MaVeCode).ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TicketCodeCreateVM());
        }

        [HttpPost]
        public async Task<IActionResult> Create(TicketCodeCreateVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var newCodes = new List<MaVe>();

            for (int i = 0; i < model.SoLuong; i++)
            {
                var code = $"{model.Prefix}{(model.StartNumber + i):000}";
                bool exists = await _context.MaVes.AnyAsync(x => x.MaVeCode == code);
                if (exists)
                {
                    ModelState.AddModelError("", $"Mã vé {code} đã tồn tại.");
                    return View(model);
                }

                newCodes.Add(new MaVe
                {
                    MaVeCode = code,
                    LoaiMaVe = model.LoaiMaVe,
                    TrangThai = "Available"
                });
            }

            _context.MaVes.AddRange(newCodes);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm mã vé thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}