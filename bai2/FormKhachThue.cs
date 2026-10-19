using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaTro.Models;
using Serilog;

namespace QuanLyNhaTro;

public class FormKhachThue : Form
{
    const int MaxNguoiMoiPhong = 2;   // quy tắc: mỗi phòng tối đa 2 người
    static readonly CultureInfo Vi = new("vi-VN");

    int? _maKhach;
    int _req;   // đánh số yêu cầu tải phường/xã để bỏ qua kết quả cũ

    TextBox txtHoTen = null!, txtSdt = null!, txtTim = null!;
    RadioButton rdoNam = null!, rdoNu = null!;
    DateTimePicker dtpNgaySinh = null!;
    ComboBox cboPhong = null!, cboTinh = null!, cboPhuongXa = null!, cboLocPhong = null!, cboLocTinh = null!;
    CheckBox chkTamTru = null!;
    DataGridView dgv = null!;
    ToolStripStatusLabel lblStatus = null!;
    Button btnLoi = null!;
    bool _dangNap;

    public FormKhachThue()
    {
        Text = "Quản lý nhà trọ - Khách thuê";
        ClientSize = new Size(1100, 560);
        MinimumSize = new Size(1100, 600);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        var g = Ui.Grp(this, "Thông tin khách thuê", 12, 10, 380, 335);
        Ui.Lbl(g, "Họ tên", 15, 28); txtHoTen = Ui.Txt(g, 110, 28, 250); txtHoTen.MaxLength = 100;
        var gg = new GroupBox { Location = new Point(110, 54), Size = new Size(250, 42) };
        rdoNam = new RadioButton { Text = "Nam", Location = new Point(12, 13), AutoSize = true, Checked = true };
        rdoNu = new RadioButton { Text = "Nữ", Location = new Point(100, 13), AutoSize = true };
        gg.Controls.Add(rdoNam); gg.Controls.Add(rdoNu); g.Controls.Add(gg);
        Ui.Lbl(g, "Ngày sinh", 15, 110); dtpNgaySinh = Ui.Dtp(g, 110, 110, 140);
        dtpNgaySinh.MaxDate = DateTime.Today; dtpNgaySinh.Value = new DateTime(2000, 1, 1);
        Ui.Lbl(g, "Số điện thoại", 15, 142); txtSdt = Ui.Txt(g, 110, 142, 250); txtSdt.MaxLength = 15;
        Ui.Lbl(g, "Phòng", 15, 174); cboPhong = Ui.Cbo(g, 110, 174, 250);
        Ui.Lbl(g, "Tỉnh/Thành", 15, 206); cboTinh = Ui.Cbo(g, 110, 206, 250);
        Ui.Lbl(g, "Phường/Xã", 15, 238); cboPhuongXa = Ui.Cbo(g, 110, 238, 250);
        chkTamTru = new CheckBox { Text = "Đã đăng ký tạm trú", Location = new Point(110, 275), AutoSize = true };
        g.Controls.Add(chkTamTru);
        cboTinh.SelectionChangeCommitted += async (s, e) =>
        {
            if (cboTinh.DataSource != null && cboTinh.SelectedValue is int code) await TaiPhuongXaAsync(code);
        };

        Ui.Btn(this, "Thêm", 12, 355, 85, btnThem_Click);
        Ui.Btn(this, "Sửa", 104, 355, 85, btnSua_Click);
        Ui.Btn(this, "Xóa", 196, 355, 85, btnXoa_Click);
        Ui.Btn(this, "Làm mới", 288, 355, 85, (s, e) => { XoaForm(); LoadData(); });
        btnLoi = Ui.Btn(this, "Gây lỗi thử", 12, 400, 120, (s, e) => throw new InvalidOperationException("Lỗi thử nghiệm do người dùng bấm nút 'Gây lỗi thử'"));
        btnLoi.Visible = false;   // ẩn; bấm Ctrl+Shift+E để hiện
        KeyDown += (s, e) => { if (e.Control && e.Shift && e.KeyCode == Keys.E) btnLoi.Visible = !btnLoi.Visible; };

        txtTim = Ui.Txt(this, 405, 12, 220); txtTim.PlaceholderText = "Tìm theo họ tên hoặc SĐT";
        txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadData(); } };
        cboLocPhong = Ui.Cbo(this, 635, 12, 110);
        cboLocTinh = Ui.Cbo(this, 755, 12, 150);
        Ui.Btn(this, "Tìm", 915, 10, 70, (s, e) => LoadData());
        Ui.Btn(this, "Bỏ lọc", 990, 10, 90, (s, e) => BoLoc());

        dgv = Ui.Grid(this, 405, 48, 683, 470);
        dgv.CellClick += Dgv_CellClick;
        Ui.Col(dgv, nameof(KhachRow.HoTen), "Họ tên", 150);
        Ui.Col(dgv, nameof(KhachRow.SoDienThoai), "SĐT", 100);
        Ui.Col(dgv, nameof(KhachRow.TenPhong), "Phòng", 60);
        Ui.Col(dgv, nameof(KhachRow.GiaThue), "Giá thuê", 100, "N0", right: true);
        Ui.Col(dgv, nameof(KhachRow.QueQuan), "Quê quán", 100, fill: true);
        dgv.Columns.Add(new DataGridViewCheckBoxColumn
        { DataPropertyName = nameof(KhachRow.DaDangKyTamTru), HeaderText = "Tạm trú", Width = 70 });

        var ss = new StatusStrip();
        lblStatus = new ToolStripStatusLabel("Sẵn sàng");
        ss.Items.Add(lblStatus);
        Controls.Add(ss);

        Load += async (s, e) =>
        {
            NapPhongCombo();
            NapLocPhong();
            LoadData();
            await TaiTinhAsync();
        };
    }

    // ================= API TỈNH / PHƯỜNG-XÃ =================
    static void DangTai(ComboBox c) { c.DataSource = null; c.DropDownStyle = ComboBoxStyle.DropDown; c.Text = "Đang tải…"; c.Enabled = false; }
    static void ThuCong(ComboBox c) { c.DataSource = null; c.DropDownStyle = ComboBoxStyle.DropDown; c.Text = ""; c.Enabled = true; }
    static bool LoiMang(Exception ex) => ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException;

    async Task TaiTinhAsync()
    {
        DangTai(cboTinh);
        cboPhuongXa.Enabled = false;
        lblStatus.Text = "Đang tải danh sách tỉnh/thành...";
        try
        {
            var ds = await DiaChiService.LayTinhAsync();
            cboTinh.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTinh.DisplayMember = "Name"; cboTinh.ValueMember = "Code";
            cboTinh.DataSource = ds;
            cboTinh.SelectedIndex = -1;
            cboTinh.Enabled = true;
            lblStatus.Text = $"Đã tải {ds.Count} tỉnh/thành từ provinces.open-api.vn";
        }
        catch (Exception ex) when (LoiMang(ex))
        {
            Log.Warning(ex, "Không tải được danh sách tỉnh/thành từ API");
            ThuCong(cboTinh); ThuCong(cboPhuongXa);
            lblStatus.Text = "Không có mạng - hãy gõ tay Tỉnh/Thành và Phường/Xã";
            Ui.Warn("Không tải được danh sách địa chỉ, vui lòng kiểm tra mạng.\nBạn vẫn có thể gõ tay Tỉnh/Thành và Phường/Xã để lưu khách.");
        }
    }

    async Task TaiPhuongXaAsync(int maTinh, int? chonMa = null, string? chonTen = null)
    {
        int id = ++_req;
        DangTai(cboPhuongXa);
        lblStatus.Text = "Đang tải phường/xã...";
        try
        {
            var ds = await DiaChiService.LayPhuongXaAsync(maTinh);
            if (id != _req) return;
            cboPhuongXa.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPhuongXa.DisplayMember = "Name"; cboPhuongXa.ValueMember = "Code";
            cboPhuongXa.DataSource = ds;
            cboPhuongXa.SelectedIndex = -1;
            cboPhuongXa.Enabled = true;
            var khop = ds.FirstOrDefault(x => chonMa != null && x.Code == chonMa) ?? ds.FirstOrDefault(x => chonTen != null && x.Name == chonTen);
            if (khop != null) cboPhuongXa.SelectedValue = khop.Code;
            lblStatus.Text = $"Đã tải {ds.Count} phường/xã";
        }
        catch (Exception ex) when (LoiMang(ex))
        {
            if (id != _req) return;
            Log.Warning(ex, "Không tải được phường/xã của tỉnh {MaTinh}", maTinh);
            ThuCong(cboPhuongXa);
            cboPhuongXa.Text = chonTen ?? "";
            lblStatus.Text = "Không tải được phường/xã - hãy gõ tay";
            Ui.Warn("Không tải được danh sách địa chỉ, vui lòng kiểm tra mạng.\nBạn vẫn có thể gõ tay Phường/Xã.");
        }
    }

    // ================= NẠP COMBOBOX PHÒNG =================
    void NapPhongCombo(int? gomPhong = null)
    {
        try
        {
            using var db = AppDb.Create();
            var dem = db.KhachThues.GroupBy(k => k.MaPhong).Select(x => new { x.Key, N = x.Count() }).ToDictionary(x => x.Key, x => x.N);
            var ds = db.PhongTros.AsNoTracking().OrderBy(p => p.TenPhong).ToList()
                .Where(p => p.MaPhong == gomPhong || dem.GetValueOrDefault(p.MaPhong) < MaxNguoiMoiPhong)
                .Select(p => new Item(p.MaPhong, $"{p.TenPhong} - {p.GiaThue.ToString("N0", Vi)} đ ({dem.GetValueOrDefault(p.MaPhong)}/{MaxNguoiMoiPhong})"))
                .ToList();
            cboPhong.DisplayMember = "Text"; cboPhong.ValueMember = "Id";
            cboPhong.DataSource = ds;
            cboPhong.SelectedIndex = ds.Count > 0 ? 0 : -1;
        }
        catch (Exception ex) { BaoLoiDb(ex, "nạp danh sách phòng"); }
    }

    void NapLocPhong()
    {
        try
        {
            using var db = AppDb.Create();
            var ds = new List<Item> { new(0, "Tất cả") };
            ds.AddRange(db.PhongTros.AsNoTracking().OrderBy(p => p.TenPhong).Select(p => new { p.MaPhong, p.TenPhong }).ToList()
                .Select(p => new Item(p.MaPhong, p.TenPhong)));
            cboLocPhong.DisplayMember = "Text"; cboLocPhong.ValueMember = "Id";
            cboLocPhong.DataSource = ds;
        }
        catch (Exception ex) { BaoLoiDb(ex, "nạp bộ lọc phòng"); }
    }

    void NapLocTinh(List<string> tinh)
    {
        _dangNap = true;
        string cu = cboLocTinh.Text;
        cboLocTinh.Items.Clear();
        cboLocTinh.Items.Add("Tất cả");
        cboLocTinh.Items.AddRange(tinh.ToArray());
        int i = cboLocTinh.FindStringExact(cu);
        cboLocTinh.SelectedIndex = i >= 0 ? i : 0;
        _dangNap = false;
    }

    // ================= TẢI DỮ LIỆU (JOIN) + LỌC =================
    void LoadData()
    {
        if (_dangNap) return;
        try
        {
            using var db = AppDb.Create();
            var tatCaTinh = db.KhachThues.Where(k => k.TenTinh != null).Select(k => k.TenTinh!).Distinct().OrderBy(x => x).ToList();
            if (cboLocTinh.Items.Count != tatCaTinh.Count + 1) NapLocTinh(tatCaTinh);

            IQueryable<KhachThue> q = db.KhachThues.AsNoTracking();
            string kw = txtTim.Text.Trim();
            if (kw.Length > 0) q = q.Where(k => k.HoTen.Contains(kw) || (k.SoDienThoai != null && k.SoDienThoai.Contains(kw)));
            if (cboLocPhong.SelectedValue is int mp && mp > 0) q = q.Where(k => k.MaPhong == mp);
            if (cboLocTinh.SelectedIndex > 0) { string t = cboLocTinh.Text; q = q.Where(k => k.TenTinh == t); }

            var rows = q.OrderBy(k => k.MaKhach).Select(k => new KhachRow
            {
                MaKhach = k.MaKhach, HoTen = k.HoTen, SoDienThoai = k.SoDienThoai,
                TenPhong = k.MaPhongNavigation.TenPhong, GiaThue = k.MaPhongNavigation.GiaThue,
                QueQuan = k.TenPhuongXa + " - " + k.TenTinh, DaDangKyTamTru = k.DaDangKyTamTru
            }).ToList();

            dgv.DataSource = rows;
            dgv.ClearSelection();
            lblStatus.Text = $"Có {rows.Count} khách";
        }
        catch (Exception ex) { BaoLoiDb(ex, "tải danh sách khách thuê"); }
    }

    void BoLoc()
    {
        txtTim.Clear();
        if (cboLocPhong.Items.Count > 0) cboLocPhong.SelectedIndex = 0;
        if (cboLocTinh.Items.Count > 0) cboLocTinh.SelectedIndex = 0;
        LoadData();
    }

    // ================= CLICK DÒNG =================
    async void Dgv_CellClick(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].DataBoundItem is not KhachRow row) return;
        try
        {
            KhachThue? k;
            using (var db = AppDb.Create()) k = db.KhachThues.AsNoTracking().FirstOrDefault(x => x.MaKhach == row.MaKhach);
            if (k is null) { LoadData(); return; }

            _maKhach = k.MaKhach;
            txtHoTen.Text = k.HoTen;
            rdoNu.Checked = k.GioiTinh == "Nữ"; rdoNam.Checked = k.GioiTinh != "Nữ";
            var ns = k.NgaySinh.ToDateTime(TimeOnly.MinValue);
            dtpNgaySinh.Value = ns > dtpNgaySinh.MaxDate ? dtpNgaySinh.MaxDate : ns;
            txtSdt.Text = k.SoDienThoai ?? "";
            chkTamTru.Checked = k.DaDangKyTamTru;
            NapPhongCombo(k.MaPhong);
            cboPhong.SelectedValue = k.MaPhong;

            if (cboTinh.DataSource is List<DiaChi> ds)
            {
                var t = ds.FirstOrDefault(x => x.Code == k.MaTinh) ?? ds.FirstOrDefault(x => x.Name == k.TenTinh);
                if (t != null)
                {
                    cboTinh.SelectedValue = t.Code;
                    await TaiPhuongXaAsync(t.Code, k.MaPhuongXa, k.TenPhuongXa);
                    return;
                }
                cboTinh.SelectedIndex = -1;
            }
            else cboTinh.Text = k.TenTinh ?? "";
            ThuCong(cboPhuongXa);
            cboPhuongXa.Text = k.TenPhuongXa ?? "";
        }
        catch (Exception ex) { BaoLoiDb(ex, "đọc thông tin khách"); }
    }

    // ================= THÊM / SỬA / XÓA (nhiều bảng) =================
    void GanDuLieu(KhachThue k)
    {
        k.HoTen = txtHoTen.Text.Trim();
        k.GioiTinh = rdoNu.Checked ? "Nữ" : "Nam";
        k.NgaySinh = DateOnly.FromDateTime(dtpNgaySinh.Value.Date);
        k.SoDienThoai = string.IsNullOrWhiteSpace(txtSdt.Text) ? null : txtSdt.Text.Trim();
        k.MaTinh = cboTinh.DataSource != null && cboTinh.SelectedValue is int mt ? mt : null;
        k.TenTinh = cboTinh.Text.Trim();
        k.MaPhuongXa = cboPhuongXa.DataSource != null && cboPhuongXa.SelectedValue is int mp ? mp : null;
        k.TenPhuongXa = cboPhuongXa.Text.Trim();
        k.DaDangKyTamTru = chkTamTru.Checked;
        k.MaPhong = (int)cboPhong.SelectedValue!;
    }

    static void CapNhatTinhTrang(QlNhaTroContext db, params int[] dsPhong)
    {
        foreach (int ma in dsPhong.Distinct())
        {
            var p = db.PhongTros.Find(ma);
            if (p is null) continue;
            p.TinhTrang = db.KhachThues.Count(x => x.MaPhong == ma) > 0 ? "Đang thuê" : "Trống";
        }
        db.SaveChanges();
    }

    void btnThem_Click(object? s, EventArgs e)
    {
        if (!HopLe()) return;
        try
        {
            using var db = AppDb.Create();
            using var tx = db.Database.BeginTransaction();
            int maPhong = (int)cboPhong.SelectedValue!;
            if (db.KhachThues.Count(x => x.MaPhong == maPhong) >= MaxNguoiMoiPhong)
            { Ui.Warn("Phòng này đã đủ người."); NapPhongCombo(); return; }
            var k = new KhachThue();
            GanDuLieu(k);
            db.KhachThues.Add(k);
            db.SaveChanges();
            CapNhatTinhTrang(db, maPhong);
            tx.Commit();
            Log.Information("Thêm khách MaKhach={MaKhach} vào phòng {MaPhong}", k.MaKhach, maPhong);
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiDb(ex, "thêm khách"); }
    }

    void btnSua_Click(object? s, EventArgs e)
    {
        if (_maKhach is null) { Ui.Warn("Hãy bấm chọn một dòng trong danh sách để sửa."); return; }
        if (!HopLe()) return;
        try
        {
            using var db = AppDb.Create();
            using var tx = db.Database.BeginTransaction();
            var k = db.KhachThues.Find(_maKhach.Value);
            if (k is null) { Ui.Warn("Khách này không còn tồn tại."); LoadData(); return; }
            int phongCu = k.MaPhong, phongMoi = (int)cboPhong.SelectedValue!;
            if (phongCu != phongMoi && db.KhachThues.Count(x => x.MaPhong == phongMoi) >= MaxNguoiMoiPhong)
            { Ui.Warn("Phòng mới đã đủ người."); return; }
            GanDuLieu(k);
            db.SaveChanges();
            CapNhatTinhTrang(db, phongCu, phongMoi);
            tx.Commit();
            Log.Information("Sửa khách MaKhach={MaKhach}", k.MaKhach);
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiDb(ex, "sửa khách"); }
    }

    void btnXoa_Click(object? s, EventArgs e)
    {
        if (_maKhach is null) { Ui.Warn("Hãy bấm chọn một dòng trong danh sách để xóa."); return; }
        if (MessageBox.Show($"Xóa khách \"{txtHoTen.Text}\"?", "Xác nhận xóa", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        try
        {
            using var db = AppDb.Create();
            using var tx = db.Database.BeginTransaction();
            var k = db.KhachThues.Find(_maKhach.Value);
            if (k is not null)
            {
                int maPhong = k.MaPhong;
                db.KhachThues.Remove(k);
                db.SaveChanges();
                CapNhatTinhTrang(db, maPhong);   // người cuối cùng rời đi -> phòng "Trống"
                Log.Information("Xóa khách MaKhach={MaKhach} khỏi phòng {MaPhong}", k.MaKhach, maPhong);
            }
            tx.Commit();
            XoaForm(); LoadData();
        }
        catch (Exception ex) { BaoLoiDb(ex, "xóa khách"); }
    }

    bool HopLe()
    {
        if (string.IsNullOrWhiteSpace(txtHoTen.Text)) { Ui.Warn("Họ tên không được để trống."); txtHoTen.Focus(); return false; }
        if (!string.IsNullOrWhiteSpace(txtSdt.Text) && !Regex.IsMatch(txtSdt.Text.Trim(), @"^\d{9,11}$"))
        { Ui.Warn("Số điện thoại phải gồm 9-11 chữ số."); txtSdt.Focus(); return false; }
        if (cboPhong.SelectedValue is not int) { Ui.Warn("Không có phòng hợp lệ để chọn (các phòng đã đủ người)."); return false; }
        if (string.IsNullOrWhiteSpace(cboTinh.Text) || cboTinh.Text == "Đang tải…") { Ui.Warn("Hãy chọn (hoặc gõ) Tỉnh/Thành."); cboTinh.Focus(); return false; }
        if (string.IsNullOrWhiteSpace(cboPhuongXa.Text) || cboPhuongXa.Text == "Đang tải…") { Ui.Warn("Hãy chọn (hoặc gõ) Phường/Xã."); cboPhuongXa.Focus(); return false; }
        return true;
    }

    void XoaForm()
    {
        _maKhach = null; _req++;
        txtHoTen.Clear(); rdoNam.Checked = true; dtpNgaySinh.Value = new DateTime(2000, 1, 1);
        txtSdt.Clear(); chkTamTru.Checked = false;
        NapPhongCombo();
        if (cboTinh.DataSource != null)
        {
            cboTinh.SelectedIndex = -1;
            cboPhuongXa.DataSource = null; cboPhuongXa.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPhuongXa.Items.Clear(); cboPhuongXa.Enabled = false;
        }
        else { cboTinh.Text = ""; cboPhuongXa.Text = ""; }
        dgv.ClearSelection(); txtHoTen.Focus();
    }

    static void BaoLoiDb(Exception ex, string hanhDong)
    {
        Log.Error(ex, "Lỗi truy cập CSDL khi {HanhDong}", hanhDong);
        Ui.Error("Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra kết nối rồi thử lại.\n(Chi tiết lỗi đã được ghi vào thư mục Logs.)");
    }
}

public class KhachRow
{
    public int MaKhach { get; set; }
    public string HoTen { get; set; } = "";
    public string? SoDienThoai { get; set; }
    public string TenPhong { get; set; } = "";
    public decimal GiaThue { get; set; }
    public string? QueQuan { get; set; }
    public bool DaDangKyTamTru { get; set; }
}
