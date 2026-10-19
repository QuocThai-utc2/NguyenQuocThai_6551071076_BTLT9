using Microsoft.EntityFrameworkCore;

namespace SoChiTieu.Models;

// Tương đương kết quả của Scaffold-DbContext (xem README nếu muốn scaffold lại)
public partial class ChiTieu
{
    public int MaCT { get; set; }
    public DateOnly NgayChi { get; set; }
    public string NoiDung { get; set; } = null!;
    public string LoaiChi { get; set; } = null!;
    public decimal SoTien { get; set; }
    public string? GhiChu { get; set; }
}

public partial class QlChiTieuContext : DbContext
{
    public QlChiTieuContext(DbContextOptions<QlChiTieuContext> options) : base(options) { }

    public virtual DbSet<ChiTieu> ChiTieus { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ChiTieu>(e =>
        {
            e.ToTable("ChiTieu");
            e.HasKey(x => x.MaCT);
            e.Property(x => x.NoiDung).HasMaxLength(100);
            e.Property(x => x.LoaiChi).HasMaxLength(30);
            e.Property(x => x.SoTien).HasPrecision(18, 0);
            e.Property(x => x.GhiChu).HasMaxLength(200);
        });
    }
}
