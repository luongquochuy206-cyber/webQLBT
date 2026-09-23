using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;

namespace QLBaiGuiXe.Controllers;

[Authorize]
public class MaintenanceController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Query = q;
        var devices = db.ThietBis.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) devices = devices.Where(x => x.Ten.Contains(q) || x.MaThietBi.Contains(q) || x.KhuVuc.Contains(q));
        ViewBag.OpenTickets = await db.PhieuSuCos.CountAsync(x => x.TrangThai != "Hoàn tất");
        ViewBag.DueSoon = await db.LichBaoTris.CountAsync(x => x.TrangThai == "Đã lên lịch" && x.NgayDuKien <= DateTime.Today.AddDays(7));
        ViewBag.Tickets = await db.PhieuSuCos.Include(x => x.ThietBi).OrderByDescending(x => x.NgayTao).Take(5).ToListAsync();
        ViewBag.Schedules = await db.LichBaoTris.Include(x => x.ThietBi).Where(x => x.TrangThai == "Đã lên lịch").OrderBy(x => x.NgayDuKien).Take(5).ToListAsync();
        return View(await devices.OrderBy(x => x.Ten).ToListAsync());
    }

    public async Task<IActionResult> Devices(string? q)
    {
        var data = db.ThietBis.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) data = data.Where(x => x.Ten.Contains(q) || x.MaThietBi.Contains(q) || x.KhuVuc.Contains(q));
        ViewBag.Query = q;
        return View(await data.OrderBy(x => x.Ten).ToListAsync());
    }
    public IActionResult CreateDevice() => View(new ThietBi());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDevice(ThietBi model)
    {
        if (await db.ThietBis.AnyAsync(x => x.MaThietBi == model.MaThietBi)) ModelState.AddModelError(nameof(model.MaThietBi), "Mã thiết bị đã tồn tại.");
        if (!ModelState.IsValid) return View(model);
        db.ThietBis.Add(model); await db.SaveChangesAsync(); TempData["Success"] = "Đã thêm thiết bị."; return RedirectToAction(nameof(Devices));
    }
    public async Task<IActionResult> EditDevice(int id) { var x = await db.ThietBis.FindAsync(id); return x == null ? NotFound() : View(x); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDevice(int id, ThietBi model)
    {
        if (id != model.Id) return NotFound();
        if (await db.ThietBis.AnyAsync(x => x.Id != id && x.MaThietBi == model.MaThietBi)) ModelState.AddModelError(nameof(model.MaThietBi), "Mã thiết bị đã tồn tại.");
        if (!ModelState.IsValid) return View(model);
        db.Update(model); await db.SaveChangesAsync(); TempData["Success"] = "Đã cập nhật thiết bị."; return RedirectToAction(nameof(Devices));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var x = await db.ThietBis.FindAsync(id); if (x == null) return NotFound();
        if (await db.LichBaoTris.AnyAsync(a => a.ThietBiId == id) || await db.PhieuSuCos.AnyAsync(a => a.ThietBiId == id)) { TempData["Error"] = "Thiết bị đã có lịch sử, không thể xóa."; return RedirectToAction(nameof(Devices)); }
        db.Remove(x); await db.SaveChangesAsync(); TempData["Success"] = "Đã xóa thiết bị."; return RedirectToAction(nameof(Devices));
    }

    public async Task<IActionResult> Tickets(string? q)
    {
        var data = db.PhieuSuCos.Include(x => x.ThietBi).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) data = data.Where(x => x.TieuDe.Contains(q) || x.ThietBi!.Ten.Contains(q) || x.TrangThai.Contains(q));
        ViewBag.Query = q; return View(await data.OrderByDescending(x => x.NgayTao).ToListAsync());
    }
    public async Task<IActionResult> CreateTicket() { ViewBag.Devices = await db.ThietBis.OrderBy(x => x.Ten).ToListAsync(); return View(new PhieuSuCo()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTicket(PhieuSuCo model)
    {
        if (!await db.ThietBis.AnyAsync(x => x.Id == model.ThietBiId)) ModelState.AddModelError(nameof(model.ThietBiId), "Vui lòng chọn thiết bị hợp lệ.");
        if (!ModelState.IsValid) { ViewBag.Devices = await db.ThietBis.OrderBy(x => x.Ten).ToListAsync(); return View(model); }
        model.NgayTao = DateTime.Now; model.MucDo = Classify(model.MoTa); db.Add(model); await db.SaveChangesAsync(); TempData["Success"] = "Đã tiếp nhận phiếu sự cố."; return RedirectToAction(nameof(Tickets));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTicket(int id, string trangThai, string nhanVienXuLy)
    {
        var x = await db.PhieuSuCos.FindAsync(id); if (x == null) return NotFound();
        x.TrangThai = trangThai; x.NhanVienXuLy = nhanVienXuLy; await db.SaveChangesAsync(); TempData["Success"] = "Đã cập nhật phiếu."; return RedirectToAction(nameof(Tickets));
    }

    public async Task<IActionResult> Schedule()
    {
        ViewBag.Devices = await db.ThietBis.OrderBy(x => x.Ten).ToListAsync();
        return View(await db.LichBaoTris.Include(x => x.ThietBi).OrderBy(x => x.NgayDuKien).ToListAsync());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSchedule(LichBaoTri model)
    {
        if (!await db.ThietBis.AnyAsync(x => x.Id == model.ThietBiId)) ModelState.AddModelError(nameof(model.ThietBiId), "Vui lòng chọn thiết bị hợp lệ.");
        if (!ModelState.IsValid) { TempData["Error"] = "Thông tin lịch chưa hợp lệ."; return RedirectToAction(nameof(Schedule)); }
        db.Add(model); await db.SaveChangesAsync(); TempData["Success"] = "Đã lên lịch bảo trì."; return RedirectToAction(nameof(Schedule));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteSchedule(int id) { var x = await db.LichBaoTris.FindAsync(id); if (x == null) return NotFound(); x.TrangThai = "Hoàn tất"; await db.SaveChangesAsync(); TempData["Success"] = "Đã hoàn tất lịch bảo trì."; return RedirectToAction(nameof(Schedule)); }

    public async Task<IActionResult> Assistant() => View(await db.ThietBis.Include(x => x.LichBaoTris).Include(x => x.PhieuSuCos).OrderBy(x => x.Ten).ToListAsync());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AskAssistant(int thietBiId, string question)
    {
        var x = await db.ThietBis.Include(a => a.LichBaoTris).Include(a => a.PhieuSuCos).FirstOrDefaultAsync(a => a.Id == thietBiId);
        if (x == null) { TempData["Error"] = "Không tìm thấy thiết bị."; return RedirectToAction(nameof(Assistant)); }
        var recent = x.PhieuSuCos.OrderByDescending(a => a.NgayTao).Take(3).ToList();
        var due = x.LichBaoTris.Where(a => a.TrangThai == "Đã lên lịch").OrderBy(a => a.NgayDuKien).FirstOrDefault();
        TempData["AiResult"] = $"{x.Ten} ({x.MaThietBi}) tại {x.KhuVuc}: trạng thái {x.TrangThai}. Có {x.PhieuSuCos.Count} phiếu sự cố trong lịch sử. " +
            (recent.Count > 0 ? "Sự cố gần đây: " + string.Join("; ", recent.Select(a => $"{a.TieuDe} [{a.MucDo}]")).Truncate(240) + ". " : "Chưa có phiếu sự cố. ") +
            (due != null ? $"Lịch bảo trì kế tiếp: {due.NgayDuKien:dd/MM/yyyy} - {due.NoiDung}. " : "Chưa có lịch bảo trì sắp tới. ") +
            "Gợi ý: kiểm tra theo lịch sử thực tế và xác nhận bởi kỹ thuật viên trước khi thao tác. (Bản demo dùng quy tắc cục bộ; chưa gọi dịch vụ AI bên ngoài.)";
        return RedirectToAction(nameof(Assistant));
    }
    private static string Classify(string text) { var s = text.ToLowerInvariant(); return new[] { "cháy", "khói", "rò điện", "ngập", "thang máy kẹt", "nguy hiểm" }.Any(s.Contains) ? "Cao" : new[] { "hỏng", "không chạy", "rò rỉ", "mất điện", "ồn" }.Any(s.Contains) ? "Trung bình" : "Thấp"; }
}

internal static class StringExtensions { public static string Truncate(this string value, int max) => value.Length <= max ? value : value[..max] + "…"; }
