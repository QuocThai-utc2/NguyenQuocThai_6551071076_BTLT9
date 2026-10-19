using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SoChiTieu.Models;

namespace SoChiTieu;

public class FormChiTieu : Form
{
    static readonly string[] DanhSachLoai = { "Ăn uống", "Đi lại", "Học tập", "Giải trí", "Khác" };
    static readonly CultureInfo Vi = new("vi-VN");

    int? _maCT;
    DateTimePicker dtpNgayChi = null!, dtpTu = null!, dtpDen = null!;
    TextBox txtNoiDung = null!, txtGhiChu = null!, txtTim = null!;
    ComboBox cboLoaiChi = null!, cboLoc = null!;
    NumericUpDown nudSoTien = null!;
    DataGridView dgv = null!;
    Label lblTong = null!;

    public FormChiTieu()
    {
        Text = "Sổ chi tiêu cá nhân";
        ClientSize = new Size(880, 570);
        MinimumSize = new Size(900, 610);
        StartPosition = FormStartPosition.CenterScreen;

        var g1 = Ui.Grp(this, "Thông tin khoản chi", 12, 10, 490, 235);
        Ui.Lbl(g1, "Ngày chi", 15, 28); dtpNgayChi = Ui.Dtp(g1, 100, 28, 140);
        Ui.Lbl(g1, "Nội dung", 15, 60); txtNoiDung = Ui.Txt(g1, 100, 60, 370); txtNoiDung.MaxLength = 100;
        Ui.Lbl(g1, "Loại chi", 15, 92); cboLoaiChi = Ui.Cbo(g1, 100, 92, 200);
        Ui.Lbl(g1, "Số tiền (đ)", 15, 124); nudSoTien = Ui.Nud(g1, 100, 124, 200, 1_000_000_000, 1000);
        Ui.Lbl(g1, "Ghi chú", 15, 156); txtGhiChu = Ui.Txt(g1, 100, 156, 370);
        txtGhiChu.Multiline = true; txtGhiChu.Height = 60; txtGhiChu.MaxLength = 200; txtGhiChu.ScrollBars = ScrollBars.Vertical;

        Ui.Btn(this, "Thêm", 12, 255, 90, btnThem_Click);
        Ui.Btn(this, "Sửa", 110, 255, 90, btnSua_Click);
        Ui.Btn(this, "Xóa", 208, 255, 90, btnXoa_Click);
        Ui.Btn(this, "Làm mới", 306, 255, 90, btnLamMoi_Click);

        var g2 = Ui.Grp(this, "Tìm kiếm", 520, 10, 348, 235);
        g2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        Ui.Lbl(g2, "Nội dung", 15, 28); txtTim = Ui.Txt(g2, 90, 28, 240); txtTim.PlaceholderText = "Nhập nội dung...";
        txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; btnTim_Click(s, EventArgs.Empty); } };
        Ui.Lbl(g2, "Loại chi", 15, 60); cboLoc = Ui.Cbo(g2, 90, 60, 240);
        Ui.Lbl(g2, "Từ ngày", 15, 92); dtpTu = Ui.Dtp(g2, 90, 92, 130);
        Ui.Lbl(g2, "Đến ngày", 15, 124); dtpDen = Ui.Dtp(g2, 90, 124, 130);
        Ui.Btn(g2, "Tìm", 90, 165, 100, btnTim_Click);

        dgv = Ui.Grid(this, 12, 300, 856, 215);
        dgv.CellClick += dgv_CellClick;
        Ui.Col(dgv, nameof(ChiTieuRow.MaCT), "Mã", 60);
        Ui.Col(dgv, nameof(ChiTieuRow.NgayChi), "Ngày chi", 100, "dd/MM/yyyy");
        Ui.Col(dgv, nameof(ChiTieuRow.NoiDung), "Nội dung", 230);
        Ui.Col(dgv, nameof(ChiTieuRow.LoaiChi), "Loại chi", 100);
        Ui.Col(dgv, nameof(ChiTieuRow.SoTien), "Số tiền", 110, "N0", right: true);
        Ui.Col(dgv, nameof(ChiTieuRow.GhiChu), "Ghi chú", 100, fill: true);

        lblTong = new Label
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            AutoSize = false,
            Size = new Size(400, 28),
            Location = new Point(468, 530),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.Firebrick,
            TextAlign = ContentAlignment.MiddleRight,
            Text = "Tổng chi: 0 đ"
        };
        Controls.Add(lblTong);

        Load += (s, e) =>
        {
            cboLoaiChi.Items.AddRange(DanhSachLoai);
            cboLoaiChi.SelectedIndex = 0;
            cboLoc.Items.Add("Tất cả");
            cboLoc.Items.AddRange(DanhSachLoai);
            cboLoc.SelectedIndex = 0;
            ResetBoLoc();
            LoadData();
        };
    }

    // ===== Tải dữ liệu + lọc =====
    void LoadData()
    {
        try
        {
            var tu = DateOnly.FromDateTime(dtpTu.Value.Date);
            var den = DateOnly.FromDateTime(dtpDen.Value.Date);
            string kw = txtTim.Text.Trim();
            string loai = cboLoc.SelectedIndex > 0 ? cboLoc.Text : "";

            using var db = AppDb.Create();
            IQueryable<ChiTieu> q = db.ChiTieus.AsNoTracking().Where(x => x.NgayChi >= tu && x.NgayChi <= den);
            if (kw.Length > 0) q = q.Where(x => x.NoiDung.Contains(kw));
            if (loai.Length > 0) q = q.Where(x => x.LoaiChi == loai);

            var rows = q.OrderBy(x => x.NgayChi).ThenBy(x => x.MaCT).ToList()
                .Select(x => new ChiTieuRow
                {
                    MaCT = x.MaCT,
                    NgayChi = x.NgayChi.ToDateTime(TimeOnly.MinValue),
                    NoiDung = x.NoiDung,
                    LoaiChi = x.LoaiChi,
                    SoTien = x.SoTien,
                    GhiChu = x.GhiChu
                }).ToList();

            dgv.DataSource = rows;
            dgv.ClearSelection();
            _maCT = null;
            lblTong.Text = $"Tổng chi: {rows.Sum(r => r.SoTien).ToString("N0", Vi)} đ";
        }
        catch (Exception ex) { BaoLoiCSDL(ex, "tải danh sách chi tiêu"); }
    }

    void btnTim_Click(object? s, EventArgs e)
    {
        if (dtpTu.Value.Date > dtpDen.Value.Date) { CanhBao("\"Từ ngày\" không được lớn hơn \"Đến ngày\"."); return; }
        LoadData();
    }

    void dgv_CellClick(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].DataBoundItem is not ChiTieuRow r) return;
        _maCT = r.MaCT;
        dtpNgayChi.Value = r.NgayChi;
        txtNoiDung.Text = r.NoiDung;
        int idx = cboLoaiChi.FindStringExact(r.LoaiChi);
        cboLoaiChi.SelectedIndex = idx >= 0 ? idx : cboLoaiChi.Items.Count - 1;
        nudSoTien.Value = Math.Min(r.SoTien, nudSoTien.Maximum);
        txtGhiChu.Text = r.GhiChu ?? "";
    }

    void btnThem_Click(object? s, EventArgs e)
    {
        if (!KiemTra()) return;
        try
        {
            using var db = AppDb.Create();
            var ct = new ChiTieu
            {
                NgayChi = DateOnly.FromDateTime(dtpNgayChi.Value.Date),
                NoiDung = txtNoiDung.Text.Trim(),
                LoaiChi = cboLoaiChi.Text,
                SoTien = nudSoTien.Value,
                GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null : txtGhiChu.Text.Trim()
            };
            db.ChiTieus.Add(ct);
            db.SaveChanges();
            Log.Information("Thêm khoản chi MaCT={MaCT}, NoiDung={NoiDung}, SoTien={SoTien}", ct.MaCT, ct.NoiDung, ct.SoTien);
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiCSDL(ex, "thêm khoản chi"); }
    }

    void btnSua_Click(object? s, EventArgs e)
    {
        if (_maCT is null) { CanhBao("Hãy bấm chọn một dòng trong danh sách để sửa."); return; }
        if (!KiemTra()) return;
        try
        {
            using var db = AppDb.Create();
            var ct = db.ChiTieus.Find(_maCT.Value);
            if (ct is null) { CanhBao("Khoản chi này không còn tồn tại."); LoadData(); return; }
            ct.NgayChi = DateOnly.FromDateTime(dtpNgayChi.Value.Date);
            ct.NoiDung = txtNoiDung.Text.Trim();
            ct.LoaiChi = cboLoaiChi.Text;
            ct.SoTien = nudSoTien.Value;
            ct.GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null : txtGhiChu.Text.Trim();
            db.SaveChanges();
            Log.Information("Sửa khoản chi MaCT={MaCT}", ct.MaCT);
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiCSDL(ex, "sửa khoản chi"); }
    }

    void btnXoa_Click(object? s, EventArgs e)
    {
        if (_maCT is null) { CanhBao("Hãy bấm chọn một dòng trong danh sách để xóa."); return; }
        var hoi = MessageBox.Show($"Bạn có chắc muốn xóa khoản chi \"{txtNoiDung.Text}\" không?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (hoi != DialogResult.Yes) return;
        try
        {
            using var db = AppDb.Create();
            var ct = db.ChiTieus.Find(_maCT.Value);
            if (ct is not null)
            {
                db.ChiTieus.Remove(ct);
                db.SaveChanges();
                Log.Information("Xóa khoản chi MaCT={MaCT}", ct.MaCT);
            }
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiCSDL(ex, "xóa khoản chi"); }
    }

    void btnLamMoi_Click(object? s, EventArgs e) { XoaForm(); ResetBoLoc(); LoadData(); }

    bool KiemTra()
    {
        if (string.IsNullOrWhiteSpace(txtNoiDung.Text)) { CanhBao("Nội dung không được để trống."); txtNoiDung.Focus(); return false; }
        if (nudSoTien.Value <= 0) { CanhBao("Số tiền phải lớn hơn 0."); nudSoTien.Focus(); return false; }
        if (dtpNgayChi.Value.Date > DateTime.Today) { CanhBao("Ngày chi không được lớn hơn hôm nay."); dtpNgayChi.Focus(); return false; }
        return true;
    }

    void XoaForm()
    {
        _maCT = null;
        dtpNgayChi.Value = DateTime.Today;
        txtNoiDung.Clear(); cboLoaiChi.SelectedIndex = 0; nudSoTien.Value = 0; txtGhiChu.Clear();
        dgv.ClearSelection(); txtNoiDung.Focus();
    }

    void ResetBoLoc()
    {
        txtTim.Clear(); cboLoc.SelectedIndex = 0;
        var h = DateTime.Today;
        dtpTu.Value = new DateTime(h.Year, h.Month, 1);
        dtpDen.Value = h;
    }

    static void CanhBao(string m) => Ui.Warn(m);

    private void InitializeComponent()
    {

    }

    static void BaoLoiCSDL(Exception ex, string hanhDong)
    {
        Log.Error(ex, "Lỗi truy cập CSDL khi {HanhDong}", hanhDong);
        Ui.Error("Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra kết nối rồi thử lại.\n(Chi tiết lỗi đã được ghi vào thư mục Logs.)");
    }
}

public class ChiTieuRow
{
    public int MaCT { get; set; }
    public DateTime NgayChi { get; set; }
    public string NoiDung { get; set; } = "";
    public string LoaiChi { get; set; } = "";
    public decimal SoTien { get; set; }
    public string? GhiChu { get; set; }
}
