using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using QLBaiGuiXe.Models;

namespace QLBaiGuiXe.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<NhanVien> NhanViens => Set<NhanVien>();
        public DbSet<MaVe> MaVes => Set<MaVe>();
        public DbSet<BangGia> BangGias => Set<BangGia>();
        public DbSet<DangKyVeThang> DangKyVeThangs => Set<DangKyVeThang>();
        public DbSet<LuotGuiXe> LuotGuiXes => Set<LuotGuiXe>();
        public DbSet<HoaDon> HoaDons => Set<HoaDon>();
        public DbSet<ThietBi> ThietBis => Set<ThietBi>();
        public DbSet<LichBaoTri> LichBaoTris => Set<LichBaoTri>();
        public DbSet<PhieuSuCo> PhieuSuCos => Set<PhieuSuCo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<NhanVien>().ToTable("NhanVien");
            modelBuilder.Entity<MaVe>().ToTable("MaVe");
            modelBuilder.Entity<BangGia>().ToTable("BangGia");
            modelBuilder.Entity<DangKyVeThang>().ToTable("DangKyVeThang");
            modelBuilder.Entity<LuotGuiXe>().ToTable("LuotGuiXe");
            modelBuilder.Entity<HoaDon>().ToTable("HoaDon");
            modelBuilder.Entity<ThietBi>().ToTable("ThietBi");
            modelBuilder.Entity<LichBaoTri>().ToTable("LichBaoTri");
            modelBuilder.Entity<PhieuSuCo>().ToTable("PhieuSuCo");
            modelBuilder.Entity<ThietBi>().HasIndex(x => x.MaThietBi).IsUnique();
            modelBuilder.Entity<LichBaoTri>().HasOne(x => x.ThietBi).WithMany(x => x.LichBaoTris).HasForeignKey(x => x.ThietBiId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PhieuSuCo>().HasOne(x => x.ThietBi).WithMany(x => x.PhieuSuCos).HasForeignKey(x => x.ThietBiId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(x => x.TenDangNhap)
                .IsUnique();

            modelBuilder.Entity<MaVe>()
                .HasIndex(x => x.MaVeCode)
                .IsUnique();

            modelBuilder.Entity<NhanVien>()
                .HasOne(x => x.TaiKhoan)
                .WithOne(x => x.NhanVien)
                .HasForeignKey<NhanVien>(x => x.TaiKhoanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DangKyVeThang>()
                .HasOne(x => x.MaVe)
                .WithMany()
                .HasForeignKey(x => x.MaVeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LuotGuiXe>()
                .HasOne(x => x.MaVe)
                .WithMany()
                .HasForeignKey(x => x.MaVeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LuotGuiXe>()
                .HasOne(x => x.DangKyVeThang)
                .WithMany()
                .HasForeignKey(x => x.DangKyVeThangId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LuotGuiXe>()
                .HasOne(x => x.NhanVienVao)
                .WithMany(x => x.LuotGuiXeVao)
                .HasForeignKey(x => x.NhanVienVaoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LuotGuiXe>()
                .HasOne(x => x.NhanVienRa)
                .WithMany(x => x.LuotGuiXeRa)
                .HasForeignKey(x => x.NhanVienRaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoaDon>()
                .HasOne(x => x.LuotGuiXe)
                .WithOne(x => x.HoaDon)
                .HasForeignKey<HoaDon>(x => x.LuotGuiXeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
