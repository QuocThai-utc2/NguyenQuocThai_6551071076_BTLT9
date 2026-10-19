using Microsoft.EntityFrameworkCore;

namespace KeHoachDaNgoai.Models;

public partial class ChuyenDi
{
    public int MaChuyen { get; set; }
    public string TenChuyen { get; set; } = null!;
    public string DiaDiem { get; set; } = null!;
    public DateOnly NgayDi { get; set; }
    public int SoNguoi { get; set; }
    public decimal NganSach { get; set; }
    public string PhuongTien { get; set; } = null!;
    public bool DaHoanThanh { get; set; }
    public string? GhiChu { get; set; }
}

public partial class QlDaNgoaiContext : DbContext
{
    public QlDaNgoaiContext(DbContextOptions<QlDaNgoaiContext> options) : base(options) { }
    public virtual DbSet<ChuyenDi> ChuyenDis { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ChuyenDi>(e =>
        {
            e.ToTable("ChuyenDi"); e.HasKey(x => x.MaChuyen);
            e.Property(x => x.TenChuyen).HasMaxLength(100);
            e.Property(x => x.DiaDiem).HasMaxLength(100);
            e.Property(x => x.NganSach).HasPrecision(18, 0);
            e.Property(x => x.PhuongTien).HasMaxLength(30);
            e.Property(x => x.GhiChu).HasMaxLength(200);
        });
    }
}
