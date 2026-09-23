using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;
using QLBaiGuiXe.ViewModels;

namespace QLBaiGuiXe.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<TaiKhoan> _hasher;

        public EmployeesController(AppDbContext context, IPasswordHasher<TaiKhoan> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var query = _context.NhanViens
                .Include(x => x.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.HoTen.Contains(keyword) ||
                    (x.SoDienThoai != null && x.SoDienThoai.Contains(keyword)) ||
                    (x.Email != null && x.Email.Contains(keyword)) ||
                    (x.TaiKhoan != null && x.TaiKhoan.TenDangNhap.Contains(keyword)));
            }

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new EmployeeCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenDangNhap = model.TenDangNhap.Trim();

            bool daTonTai = await _context.TaiKhoans.AnyAsync(x => x.TenDangNhap == tenDangNhap);
            if (daTonTai)
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            if (model.VaiTro != "Admin" && model.VaiTro != "NhanVien")
            {
                ModelState.AddModelError("", "Vai trò không hợp lệ.");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    VaiTro = model.VaiTro,
                    TrangThai = true
                };

                taiKhoan.MatKhauHash = _hasher.HashPassword(taiKhoan, model.MatKhau);

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                var nhanVien = new NhanVien
                {
                    HoTen = model.HoTen.Trim(),
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    TaiKhoanId = taiKhoan.Id
                };

                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "Thêm nhân viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Lỗi khi thêm nhân viên: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var nv = await _context.NhanViens
                .Include(x => x.TaiKhoan)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (nv == null || nv.TaiKhoan == null)
                return NotFound();

            var vm = new EmployeeEditVM
            {
                Id = nv.Id,
                TaiKhoanId = nv.TaiKhoanId,
                HoTen = nv.HoTen,
                SoDienThoai = nv.SoDienThoai,
                Email = nv.Email,
                TenDangNhap = nv.TaiKhoan.TenDangNhap,
                VaiTro = nv.TaiKhoan.VaiTro,
                TrangThai = nv.TaiKhoan.TrangThai
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeEditVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nv = await _context.NhanViens
                .Include(x => x.TaiKhoan)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (nv == null || nv.TaiKhoan == null)
                return NotFound();

            var tenDangNhap = model.TenDangNhap.Trim();

            bool daTonTai = await _context.TaiKhoans
                .AnyAsync(x => x.TenDangNhap == tenDangNhap && x.Id != nv.TaiKhoanId);

            if (daTonTai)
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            if (model.VaiTro != "Admin" && model.VaiTro != "NhanVien")
            {
                ModelState.AddModelError("", "Vai trò không hợp lệ.");
                return View(model);
            }

            try
            {
                nv.HoTen = model.HoTen.Trim();
                nv.SoDienThoai = model.SoDienThoai;
                nv.Email = model.Email;

                nv.TaiKhoan.TenDangNhap = tenDangNhap;
                nv.TaiKhoan.VaiTro = model.VaiTro;
                nv.TaiKhoan.TrangThai = model.TrangThai;

                if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
                {
                    nv.TaiKhoan.MatKhauHash = _hasher.HashPassword(nv.TaiKhoan, model.MatKhauMoi);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật nhân viên thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi sửa nhân viên: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nv = await _context.NhanViens
                .Include(x => x.TaiKhoan)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (nv == null || nv.TaiKhoan == null)
                return NotFound();

            bool coDuLieuPhatSinh = await _context.LuotGuiXes
                .AnyAsync(x => x.NhanVienVaoId == id || x.NhanVienRaId == id);

            try
            {
                if (coDuLieuPhatSinh)
                {
                    nv.TaiKhoan.TrangThai = false;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Nhân viên đã có dữ liệu phát sinh, hệ thống khóa tài khoản thay vì xóa.";
                }
                else
                {
                    _context.NhanViens.Remove(nv);
                    _context.TaiKhoans.Remove(nv.TaiKhoan);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa nhân viên thành công.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa nhân viên: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}