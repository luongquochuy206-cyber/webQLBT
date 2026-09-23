using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;
using QLBaiGuiXe.ViewModels;

namespace QLBaiGuiXe.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<TaiKhoan> _hasher;

        public AccountController(AppDbContext context, IPasswordHasher<TaiKhoan> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var taiKhoan = await _context.TaiKhoans
                .Include(x => x.NhanVien)
                .FirstOrDefaultAsync(x => x.TenDangNhap == model.TenDangNhap);

            if (taiKhoan == null || !taiKhoan.TrangThai)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View(model);
            }

            var result = _hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhauHash, model.MatKhau);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.Id.ToString()),
                new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taiKhoan.VaiTro)
            };

            if (taiKhoan.NhanVien != null)
            {
                claims.Add(new Claim("NhanVienId", taiKhoan.NhanVien.Id.ToString()));
                claims.Add(new Claim("HoTen", taiKhoan.NhanVien.HoTen));
            }
            else
            {
                var student = await _context.SinhViens.FirstOrDefaultAsync(x => x.MaSinhVien == taiKhoan.TenDangNhap);
                if (student != null)
                {
                    claims.Add(new Claim("HoTen", student.HoTen));
                    claims.Add(new Claim("SinhVienId", student.Id.ToString()));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["Success"] = "Đăng nhập thành công";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
