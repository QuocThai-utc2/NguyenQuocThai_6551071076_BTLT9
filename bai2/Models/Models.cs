using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaTro.Models;

// Tương đương kết quả Scaffold-DbContext (có navigation property giữa 2 bảng)
public partial class PhongTro
{
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = null!;
    public decimal GiaThue { get; set; }
    public string TinhTrang { get; set; } = null!;
    public virtual ICollection<KhachThue> KhachThues { get; set; } = new List<KhachThue>();
}

public partial class KhachThue
{
    public int MaKhach { get; set; }
    public string HoTen { get; set; } = null!;
    public string GioiTinh { get; set; } = null!;
    public DateOnly NgaySinh { get; set; }
    public string? SoDienThoai { get; set; }
    public int? MaTinh { get; set; }
    public string? TenTinh { get; set; }
    public int? MaPhuongXa { get; set; }
    public string? TenPhuongXa { get; set; }
    public bool DaDangKyTamTru { get; set; }
    public int MaPhong { get; set; }
    public virtual PhongTro MaPhongNavigation { get; set; } = null!;
}

public partial class QlNhaTroContext : DbContext
{
    public QlNhaTroContext(DbContextOptions<QlNhaTroContext> options) : base(options) { }

    public virtual DbSet<PhongTro> PhongTros { get; set; }
    public virtual DbSet<KhachThue> KhachThues { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<PhongTro>(e =>
        {
            e.ToTable("PhongTro"); e.HasKey(x => x.MaPhong);
            e.Property(x => x.TenPhong).HasMaxLength(20);
            e.Property(x => x.GiaThue).HasPrecision(18, 0);
            e.Property(x => x.TinhTrang).HasMaxLength(20);
        });
        mb.Entity<KhachThue>(e =>
        {
            e.ToTable("KhachThue"); e.HasKey(x => x.MaKhach);
            e.Property(x => x.HoTen).HasMaxLength(100);
            e.Property(x => x.GioiTinh).HasMaxLength(5);
            e.Property(x => x.SoDienThoai).HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.TenTinh).HasMaxLength(100);
            e.Property(x => x.TenPhuongXa).HasMaxLength(100);
            e.HasOne(x => x.MaPhongNavigation).WithMany(p => p.KhachThues)
                .HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhachThue_PhongTro");
        });
    }
}
