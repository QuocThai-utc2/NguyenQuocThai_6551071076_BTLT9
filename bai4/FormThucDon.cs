using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BanHangTraSua.Models;

namespace BanHangTraSua;

public class FormThucDon : Form
{
    static readonly CultureInfo Vi = new("vi-VN");
    int? _ma;

    TextBox txtTen = null!, txtTim = null!;
    ComboBox cboDM = null!, cboLoc = null!;
    NumericUpDown nudGia = null!;
    CheckBox chkDangBan = null!;
    DataGridView dgv = null!;

    public FormThucDon()
    {
        Text = "Quản lý thực đơn";
        ClientSize = new Size(900, 580);
        MinimumSize = new Size(920, 620);
        StartPosition = FormStartPosition.CenterParent;

        var g1 = Ui.Grp(this, "Thông tin món", 12, 10, 420, 175);
        Ui.Lbl(g1, "Tên món", 15, 28);   txtTen = Ui.Txt(g1, 110, 28, 290); txtTen.MaxLength = 100;
        Ui.Lbl(g1, "Danh mục", 15, 60);  cboDM = Ui.Cbo(g1, 110, 60, 200);
        Ui.Lbl(g1, "Giá bán (đ)", 15, 92); nudGia = Ui.Nud(g1, 110, 92, 160, 10_000_000, 1000);
        chkDangBan = new CheckBox { Text = "Đang bán", Location = new Point(110, 128), AutoSize = true, Checked = true };
        g1.Controls.Add(chkDangBan);

        Ui.Btn(this, "Thêm", 12, 195, 90, btnThem_Click);
        Ui.Btn(this, "Sửa", 110, 195, 90, btnSua_Click);
        Ui.Btn(this, "Xóa", 208, 195, 90, btnXoa_Click);
        Ui.Btn(this, "Làm mới", 306, 195, 90, (s, e) => { XoaForm(); txtTim.Clear(); cboLoc.SelectedIndex = 0; LoadData(); });

        var g2 = Ui.Grp(this, "Tìm kiếm", 450, 10, 438, 175);
        g2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        Ui.Lbl(g2, "Tên món", 15, 28);  txtTim = Ui.Txt(g2, 100, 28, 320);
        txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadData(); } };
        Ui.Lbl(g2, "Danh mục", 15, 60); cboLoc = Ui.Cbo(g2, 100, 60, 200);
        Ui.Btn(g2, "Tìm", 100, 100, 100, (s, e) => LoadData());

        dgv = Ui.Grid(this, 12, 240, 876, 330);
        dgv.CellClick += Dgv_CellClick;
        Ui.Col(dgv, nameof(MonRow.MaMon), "Mã", 60);
        Ui.Col(dgv, nameof(MonRow.TenMon), "Tên món", 260);
        Ui.Col(dgv, nameof(MonRow.TenDM), "Danh mục", 160);
        Ui.Col(dgv, nameof(MonRow.GiaBan), "Giá bán", 110, "N0", right: true);
        dgv.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(MonRow.DangBan), HeaderText = "Đang bán", Width = 90 });

        Load += (s, e) => { NapDanhMuc(); LoadData(); };
    }

    void NapDanhMuc()
    {
        try
        {
            using var db = AppDb.Create();
            var dm = db.DanhMucMons.AsNoTracking().OrderBy(d => d.MaDM).ToList().Select(d => new Item(d.MaDM, d.TenDM)).ToList();
            cboDM.DisplayMember = "Text"; cboDM.ValueMember = "Id";
            cboDM.DataSource = dm.ToList();
            var loc = new List<Item> { new(0, "Tất cả") }; loc.AddRange(dm);
            cboLoc.DisplayMember = "Text"; cboLoc.ValueMember = "Id";
            cboLoc.DataSource = loc;
        }
        catch (Exception ex) { BaoLoiDb(ex, "nạp danh mục"); }
    }

    void LoadData()
    {
        try
        {
            using var db = AppDb.Create();
            IQueryable<MonUong> q = db.MonUongs.AsNoTracking();
            string kw = txtTim.Text.Trim();
            if (kw.Length > 0) q = q.Where(m => m.TenMon.Contains(kw));
            if (cboLoc.SelectedValue is int dm && dm > 0) q = q.Where(m => m.MaDM == dm);

            var rows = q.OrderBy(m => m.MaDMNavigation.TenDM).ThenBy(m => m.TenMon)
                .Select(m => new MonRow
                { MaMon = m.MaMon, TenMon = m.TenMon, MaDM = m.MaDM, TenDM = m.MaDMNavigation.TenDM, GiaBan = m.GiaBan, DangBan = m.DangBan })
                .ToList();
            dgv.DataSource = rows;
            dgv.ClearSelection(); _ma = null;
        }
        catch (Exception ex) { BaoLoiDb(ex, "tải thực đơn"); }
    }

    void Dgv_CellClick(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].DataBoundItem is not MonRow r) return;
        _ma = r.MaMon;
        txtTen.Text = r.TenMon;
        cboDM.SelectedValue = r.MaDM;
        nudGia.Value = Math.Min(r.GiaBan, nudGia.Maximum);
        chkDangBan.Checked = r.DangBan;
    }

    bool HopLe()
    {
        if (string.IsNullOrWhiteSpace(txtTen.Text)) { Ui.Warn("Tên món không được để trống."); txtTen.Focus(); return false; }
        if (cboDM.SelectedValue is not int) { Ui.Warn("Hãy chọn danh mục."); return false; }
        if (nudGia.Value <= 0) { Ui.Warn("Giá bán phải lớn hơn 0."); nudGia.Focus(); return false; }
        return true;
    }

    void btnThem_Click(object? s, EventArgs e)
    {
        if (!HopLe()) return;
        try
        {
            using var db = AppDb.Create();
            var m = new MonUong { TenMon = txtTen.Text.Trim(), MaDM = (int)cboDM.SelectedValue!, GiaBan = nudGia.Value, DangBan = chkDangBan.Checked };
            db.MonUongs.Add(m); db.SaveChanges();
            Log.Information("Thêm món MaMon={MaMon}, {TenMon}", m.MaMon, m.TenMon);
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiDb(ex, "thêm món"); }
    }

    void btnSua_Click(object? s, EventArgs e)
    {
        if (_ma is null) { Ui.Warn("Hãy chọn một dòng để sửa."); return; }
        if (!HopLe()) return;
        try
        {
            using var db = AppDb.Create();
            var m = db.MonUongs.Find(_ma.Value);
            if (m is null) { LoadData(); return; }
            m.TenMon = txtTen.Text.Trim(); m.MaDM = (int)cboDM.SelectedValue!; m.GiaBan = nudGia.Value; m.DangBan = chkDangBan.Checked;
            db.SaveChanges();
            Log.Information("Sửa món MaMon={MaMon}", m.MaMon);
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiDb(ex, "sửa món"); }
    }

    void btnXoa_Click(object? s, EventArgs e)
    {
        if (_ma is null) { Ui.Warn("Hãy chọn một dòng để xóa."); return; }
        if (MessageBox.Show($"Xóa món \"{txtTen.Text}\"?", "Xác nhận xóa", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        try
        {
            using var db = AppDb.Create();
            int ma = _ma.Value;
            if (db.ChiTietHoaDons.Any(c => c.MaMon == ma))
            {
                Ui.Warn("Món này đã có trong hóa đơn nên không thể xóa.\nHãy bỏ chọn \"Đang bán\" rồi bấm Sửa để ẩn món.");
                return;
            }
            var m = db.MonUongs.Find(ma);
            if (m is not null) { db.MonUongs.Remove(m); db.SaveChanges(); Log.Information("Xóa món MaMon={MaMon}", ma); }
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiDb(ex, "xóa món"); }
    }

    void XoaForm()
    {
        _ma = null; txtTen.Clear(); nudGia.Value = 0; chkDangBan.Checked = true;
        if (cboDM.Items.Count > 0) cboDM.SelectedIndex = 0;
        dgv.ClearSelection(); txtTen.Focus();
    }

    static void BaoLoiDb(Exception ex, string hanhDong)
    {
        Log.Error(ex, "Lỗi truy cập CSDL khi {HanhDong}", hanhDong);
        Ui.Error("Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra kết nối rồi thử lại.\n(Chi tiết lỗi đã được ghi vào thư mục Logs.)");
    }
}

public class MonRow
{
    public int MaMon { get; set; }
    public string TenMon { get; set; } = "";
    public int MaDM { get; set; }
    public string TenDM { get; set; } = "";
    public decimal GiaBan { get; set; }
    public bool DangBan { get; set; }
}
