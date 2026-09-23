using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Data;
using QLBaiGuiXe.Models;

namespace QLBaiGuiXe.Controllers;

[Authorize]
public class AcademicController(AppDbContext db, IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
{
    private async Task<SinhVien?> CurrentStudentAsync()
    {
        var username = User.Identity?.Name;
        var student = string.IsNullOrWhiteSpace(username) ? null : await db.SinhViens.FirstOrDefaultAsync(x => x.MaSinhVien == username);
        return student ?? (User.IsInRole("SinhVien") ? null : await db.SinhViens.FirstOrDefaultAsync());
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Students(string? q)
    {
        ViewBag.Query = q;
        var query = db.SinhViens.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.HoTen.Contains(q) || x.MaSinhVien.Contains(q) || x.Nganh.Contains(q));
        return View(await query.OrderBy(x => x.MaSinhVien).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveStudent(SinhVien student)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Vui lòng kiểm tra lại thông tin sinh viên."; return RedirectToAction(nameof(Students)); }
        if (await db.SinhViens.AnyAsync(x => x.MaSinhVien == student.MaSinhVien && x.Id != student.Id)) { TempData["Error"] = "Mã sinh viên đã tồn tại."; return RedirectToAction(nameof(Students)); }
        if (student.Id == 0) db.SinhViens.Add(student); else db.SinhViens.Update(student);
        await db.SaveChangesAsync(); TempData["Success"] = "Đã lưu hồ sơ sinh viên."; return RedirectToAction(nameof(Students));
    }

    public async Task<IActionResult> Courses(string? q)
    {
        ViewBag.Query = q;
        var query = db.HocPhans.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.TenHocPhan.Contains(q) || x.MaHocPhan.Contains(q));
        ViewBag.Classes = await db.LopHocPhans.Include(x => x.HocPhan).Include(x => x.DangKys).OrderBy(x => x.Thu).ThenBy(x => x.TietBatDau).ToListAsync();
        return View(await query.OrderBy(x => x.MaHocPhan).ToListAsync());
    }

    public async Task<IActionResult> Enrollments()
    {
        var student = await CurrentStudentAsync() ?? new SinhVien();
        ViewBag.Student = student;
        ViewBag.OpenClasses = await db.LopHocPhans.Include(x => x.HocPhan).Include(x => x.DangKys).OrderBy(x => x.Thu).ThenBy(x => x.TietBatDau).ToListAsync();
        ViewBag.Enrolled = await db.DangKyHocPhans.Include(x => x.LopHocPhan!).ThenInclude(x => x.HocPhan).Where(x => x.SinhVienId == student.Id).ToListAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int classId)
    {
        var student = await CurrentStudentAsync();
        var section = await db.LopHocPhans.Include(x => x.HocPhan).Include(x => x.DangKys).FirstOrDefaultAsync(x => x.Id == classId);
        if (student == null || section == null) TempData["Error"] = "Không tìm thấy sinh viên hoặc lớp học phần.";
        else if (section.DangKys.Any(x => x.SinhVienId == student.Id)) TempData["Error"] = "Bạn đã đăng ký lớp học phần này.";
        else if (section.DangKys.Count >= section.SiSoToiDa) TempData["Error"] = "Lớp đã đủ sĩ số.";
        else
        {
            var completed = await db.DangKyHocPhans.Include(x => x.LopHocPhan!).ThenInclude(x => x.HocPhan).Where(x => x.SinhVienId == student.Id && x.DiemQuaTrinh != null && x.DiemThi != null && (x.DiemQuaTrinh.Value * .4 + x.DiemThi.Value * .6) >= 4).Select(x => x.LopHocPhan!.HocPhan!.MaHocPhan).ToListAsync();
            var current = await db.DangKyHocPhans.Include(x => x.LopHocPhan).Where(x => x.SinhVienId == student.Id).Select(x => x.LopHocPhan!).ToListAsync();
            var prerequisite = section.HocPhan?.MaTienQuyet;
            var missing = prerequisite != null && !completed.Contains(prerequisite) && await db.HocPhans.AnyAsync(x => x.MaHocPhan == prerequisite);
            var conflict = current.Any(x => x.Thu == section.Thu && x.TietBatDau < section.TietBatDau + section.SoTiet && section.TietBatDau < x.TietBatDau + x.SoTiet);
            if (missing) TempData["Error"] = $"Chưa đạt học phần tiên quyết {section.HocPhan!.MaTienQuyet}.";
            else if (conflict) TempData["Error"] = "Lịch học bị trùng với lớp bạn đã đăng ký.";
            else { db.DangKyHocPhans.Add(new DangKyHocPhan { SinhVienId = student.Id, LopHocPhanId = classId }); await db.SaveChangesAsync(); TempData["Success"] = "Đăng ký học phần thành công."; }
        }
        return RedirectToAction(nameof(Enrollments));
    }

    public async Task<IActionResult> Grades()
    {
        var student = await CurrentStudentAsync();
        var grades = student == null ? new List<DangKyHocPhan>() : await db.DangKyHocPhans.Include(x => x.LopHocPhan!).ThenInclude(x => x.HocPhan).Where(x => x.SinhVienId == student.Id).ToListAsync();
        ViewBag.Student = student; return View(grades);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> SaveGrade(int id, double? diemQuaTrinh, double? diemThi)
    {
        if ((diemQuaTrinh.HasValue && (diemQuaTrinh < 0 || diemQuaTrinh > 10)) || (diemThi.HasValue && (diemThi < 0 || diemThi > 10)))
            TempData["Error"] = "Điểm phải nằm trong khoảng từ 0 đến 10.";
        else
        {
            var enrollment = await db.DangKyHocPhans.FindAsync(id);
            if (enrollment == null) TempData["Error"] = "Không tìm thấy đăng ký học phần.";
            else
            {
                enrollment.DiemQuaTrinh = diemQuaTrinh;
                enrollment.DiemThi = diemThi;
                enrollment.TrangThai = diemQuaTrinh.HasValue && diemThi.HasValue ? "Đã hoàn thành" : "Đang học";
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật điểm. Điểm tổng kết được tính tự động.";
            }
        }
        return RedirectToAction(nameof(Grades));
    }

    [HttpGet]
    public IActionResult Assistant() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assistant(string question)
    {
        ViewBag.Question = question;
        if (string.IsNullOrWhiteSpace(question)) { ViewBag.Answer = "Bạn hãy nhập câu hỏi về đăng ký học phần, lịch học hoặc kết quả học tập."; return View(); }
        var student = await CurrentStudentAsync();
        var courses = await db.HocPhans.OrderBy(x => x.MaHocPhan).ToListAsync();
        var enrolled = student == null ? new List<DangKyHocPhan>() : await db.DangKyHocPhans.Include(x => x.LopHocPhan!).ThenInclude(x => x.HocPhan).Where(x => x.SinhVienId == student.Id).ToListAsync();
        var q = question.ToLowerInvariant();
        if (q.Contains("điểm") || q.Contains("học tập"))
        {
            var scored = enrolled.Where(x => x.DiemTongKet.HasValue).ToList();
            ViewBag.Answer = scored.Count == 0 ? "Hồ sơ hiện chưa có điểm tổng kết. Khi có điểm, hệ thống sẽ giúp bạn xem tiến độ học tập." : $"Bạn đã có điểm ở {scored.Count} học phần. Điểm trung bình hiện tại là {scored.Average(x => x.DiemTongKet):0.0}/10. Hãy ưu tiên cải thiện học phần dưới 5.0.";
        }
        else if (q.Contains("tiên quyết") || q.Contains("đăng ký") || q.Contains("môn"))
            ViewBag.Answer = $"Kỳ này có {courses.Count} học phần trong danh mục và bạn đang đăng ký {enrolled.Count} lớp. Hệ thống kiểm tra học phần tiên quyết, sĩ số và trùng lịch trước khi xác nhận. Ví dụ: {string.Join(", ", courses.Take(4).Select(x => x.MaHocPhan + " – " + x.TenHocPhan))}.";
        else ViewBag.Answer = "Bạn có thể hỏi về điều kiện đăng ký, học phần tiên quyết, lịch học hoặc điểm số. Quy chế và dữ liệu cá nhân cần được đối chiếu với phòng đào tạo; trợ lý không thay thế quyết định chính thức.";
        var apiKey = configuration["AI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var context = $"Học phần hiện có: {string.Join("; ", courses.Select(x => $"{x.MaHocPhan} {x.TenHocPhan} ({x.TinChi} tín chỉ, tiên quyết {x.MaTienQuyet ?? "không"})"))}.";
                if (q.Contains("điểm") || q.Contains("học tập"))
                    context += $" Kết quả học tập của sinh viên hiện đăng nhập: {string.Join("; ", enrolled.Select(x => $"{x.LopHocPhan?.HocPhan?.TenHocPhan}: {(x.DiemTongKet?.ToString("0.0") ?? "đang học")}"))}.";
                var endpoint = configuration["AI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
                var model = configuration["AI:Model"] ?? "gpt-4o-mini";
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = System.Net.Http.Json.JsonContent.Create(new { model, temperature = 0.2, messages = new[] {
                    new { role = "system", content = "Bạn là trợ lý học vụ. Chỉ tư vấn từ dữ liệu được cung cấp, không tự bịa quy chế. Hãy nêu rõ khi thiếu dữ liệu và không thay thế quyết định của phòng đào tạo. Trả lời ngắn gọn bằng tiếng Việt." },
                    new { role = "user", content = $"Dữ liệu được phép dùng cho phiên tư vấn này: {context}\nCâu hỏi: {question}" }
                } });
                using var timeout = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
                timeout.CancelAfter(TimeSpan.FromSeconds(15));
                using var response = await httpClientFactory.CreateClient().SendAsync(request, timeout.Token);
                response.EnsureSuccessStatusCode();
                using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
                var answer = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                if (!string.IsNullOrWhiteSpace(answer)) ViewBag.Answer = answer;
            }
            catch (Exception) { ViewBag.AiNotice = "Không kết nối được dịch vụ AI; đang hiển thị gợi ý demo cục bộ."; }
        }
        return View();
    }
}
