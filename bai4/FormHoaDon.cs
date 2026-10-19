using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BanHangTraSua;

public class FormHoaDon : Form
{
    static readonly CultureInfo Vi = new("vi-VN");
    DateTimePicker dtp = null!;
    DataGridView dgvHD = null!, dgvCT = null!;
    Label lblDoanhThu = null!;

    public FormHoaDon()
    {
        Text = "Danh sách hóa đơn";
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(920, 640);
        StartPosition = FormStartPosition.CenterParent;

        Ui.Lbl(this, "Ngày:", 12, 12);
        dtp = Ui.Dtp(this, 60, 12, 130);
        dtp.Value = DateTime.Today;
        dtp.ValueChanged += (s, e) => LoadHoaDon();

        lblDoanhThu = new Label
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right, AutoSize = false, Size = new Size(450, 28), Location = new Point(438, 12),
            TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.Firebrick,
            Text = "Doanh thu ngày: 0 đ"
        };
        Controls.Add(lblDoanhThu);

        dgvHD = Ui.Grid(this, 12, 50, 876, 240);
        dgvHD.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        Ui.Col(dgvHD, nameof(HdRow.MaHD), "Mã HD", 80);
        Ui.Col(dgvHD, nameof(HdRow.GioLap), "Giờ lập", 140, "dd/MM/yyyy HH:mm");
        Ui.Col(dgvHD, nameof(HdRow.HinhThuc), "Hình thức", 150);
        Ui.Col(dgvHD, nameof(HdRow.TongTien), "Tổng tiền", 150, "N0", right: true);
        dgvHD.SelectionChanged += (s, e) => LoadChiTiet();

        Ui.Lbl(this, "Chi tiết hóa đơn đang chọn:", 12, 300);
        dgvCT = Ui.Grid(this, 12, 330, 876, 260);
        Ui.Col(dgvCT, nameof(CtRow.TenMon), "Tên món", 300);
        Ui.Col(dgvCT, nameof(CtRow.SoLy), "SL", 60);
        Ui.Col(dgvCT, nameof(CtRow.DonGia), "Đơn giá", 120, "N0", right: true);
        Ui.Col(dgvCT, nameof(CtRow.ThanhTien), "Thành tiền", 140, "N0", right: true);

        Load += (s, e) => LoadHoaDon();
    }

    void LoadHoaDon()
    {
        try
        {
            var d1 = dtp.Value.Date; var d2 = d1.AddDays(1);
            using var db = AppDb.Create();
            var rows = db.HoaDons.AsNoTracking().Where(h => h.NgayLap >= d1 && h.NgayLap < d2).OrderBy(h => h.NgayLap)
                .Select(h => new HdRow { MaHD = h.MaHD, GioLap = h.NgayLap, HinhThuc = h.HinhThucTT, TongTien = h.TongTien, DaThanhToan = h.DaThanhToan })
                .ToList();
            dgvHD.DataSource = rows;
            lblDoanhThu.Text = $"Doanh thu ngày: {rows.Where(r => r.DaThanhToan).Sum(r => r.TongTien).ToString("N0", Vi)} đ";
            LoadChiTiet();
        }
        catch (Exception ex) { BaoLoiDb(ex, "tải hóa đơn"); }
    }

    void LoadChiTiet()
    {
        if (dgvHD.CurrentRow?.DataBoundItem is not HdRow hd || !dgvHD.CurrentRow.Selected) { dgvCT.DataSource = null; return; }
        try
        {
            int ma = hd.MaHD;
            using var db = AppDb.Create();
            dgvCT.DataSource = db.ChiTietHoaDons.AsNoTracking().Where(c => c.MaHD == ma)
                .Select(c => new CtRow { TenMon = c.MaMonNavigation.TenMon, SoLy = c.SoLy, DonGia = c.DonGia, ThanhTien = c.SoLy * c.DonGia })
                .ToList();
        }
        catch (Exception ex) { BaoLoiDb(ex, "tải chi tiết hóa đơn"); }
    }

    static void BaoLoiDb(Exception ex, string hanhDong)
    {
        Log.Error(ex, "Lỗi truy cập CSDL khi {HanhDong}", hanhDong);
        Ui.Error("Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra kết nối rồi thử lại.\n(Chi tiết lỗi đã được ghi vào thư mục Logs.)");
    }
}

public class HdRow
{
    public int MaHD { get; set; }
    public DateTime GioLap { get; set; }
    public string HinhThuc { get; set; } = "";
    public decimal TongTien { get; set; }
    public bool DaThanhToan { get; set; }
}

public class CtRow
{
    public string TenMon { get; set; } = "";
    public int SoLy { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}
