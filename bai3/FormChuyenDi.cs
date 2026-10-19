using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using KeHoachDaNgoai.Models;

namespace KeHoachDaNgoai;

public class FormChuyenDi : Form
{
    static readonly (string Value, string Key)[] Transports =
        { ("Xe máy", "t_moto"), ("Ô tô", "t_car"), ("Xe khách", "t_coach"), ("Tàu hỏa", "t_train") };
    record TItem(string Value, string Text) { public override string ToString() => Text; }

    static string T(string k) => Lang.T(k);
    static string T(string k, params object[] a) => Lang.T(k, a);

    readonly List<(Control C, string Key)> _tr = new();
    int? _ma;
    Action? _render;                       // vẽ lại panel thời tiết khi đổi ngôn ngữ
    CancellationTokenSource? _cts;

    TextBox txtTen = null!, txtDiaDiem = null!, txtTim = null!;
    DateTimePicker dtpNgayDi = null!, dtpTu = null!, dtpDen = null!;
    NumericUpDown nudSoNguoi = null!, nudNganSach = null!;
    ComboBox cboPT = null!, cboLang = null!;
    CheckBox chkDone = null!, chkChuaHT = null!, chkLocNgay = null!;
    DataGridView dgv = null!;
    Button btnWeather = null!;
    ProgressBar prg = null!;
    Label lblHead = null!, lblIcon = null!, lblDesc = null!, lblTemp = null!, lblRain = null!, lblAdvice = null!;
    Panel pnlResult = null!, pnlAdvice = null!;

    TC Tr<TC>(TC c, string key) where TC : Control { _tr.Add((c, key)); return c; }

    public FormChuyenDi()
    {
        ClientSize = new Size(1000, 680);
        MinimumSize = new Size(1000, 720);
        StartPosition = FormStartPosition.CenterScreen;

        var title = Tr(new Label { AutoSize = true, Location = new Point(12, 8), Font = new Font("Segoe UI", 15F, FontStyle.Bold) }, "title");
        Controls.Add(title);
        var lblLang = Tr(new Label { AutoSize = true, Location = new Point(770, 14), Anchor = AnchorStyles.Top | AnchorStyles.Right }, "lang");
        Controls.Add(lblLang);
        cboLang = Ui.Cbo(this, 860, 10, 128);
        cboLang.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cboLang.Items.AddRange(new object[] { "Tiếng Việt", "English" });
        cboLang.SelectedIndex = Lang.Code == "en" ? 1 : 0;
        cboLang.SelectedIndexChanged += (s, e) =>
        {
            Lang.Set(cboLang.SelectedIndex == 1 ? "en" : "vi");
            Lang.Save();                  // lưu vào %LocalAppData%\KeHoachDaNgoai\usersettings.json
            ApplyLanguage();
        };

        // ---- Thông tin chuyến đi ----
        var g = Tr(Ui.Grp(this, "", 12, 45, 470, 275), "grpInfo");
        Tr(Ui.Lbl(g, "", 15, 25), "name");      txtTen = Ui.Txt(g, 130, 25, 320); txtTen.MaxLength = 100;
        Tr(Ui.Lbl(g, "", 15, 55), "place");     txtDiaDiem = Ui.Txt(g, 130, 55, 320); txtDiaDiem.MaxLength = 100;
        Tr(Ui.Lbl(g, "", 15, 85), "date");      dtpNgayDi = Ui.Dtp(g, 130, 85, 140);
        Tr(Ui.Lbl(g, "", 15, 115), "people");   nudSoNguoi = Ui.Nud(g, 130, 115, 100, 1000, 1, 1);
        Tr(Ui.Lbl(g, "", 15, 145), "budget");   nudNganSach = Ui.Nud(g, 130, 145, 180, 10_000_000_000, 100_000, 0);
        Tr(Ui.Lbl(g, "", 15, 175), "transport"); cboPT = Ui.Cbo(g, 130, 175, 180);
        chkDone = Tr(new CheckBox { AutoSize = true, Location = new Point(130, 207) }, "done");
        g.Controls.Add(chkDone);
        Tr(Ui.Btn(g, "", 15, 235, 100, btnThem_Click), "add");
        Tr(Ui.Btn(g, "", 123, 235, 100, btnSua_Click), "edit");
        Tr(Ui.Btn(g, "", 231, 235, 100, btnXoa_Click), "del");
        Tr(Ui.Btn(g, "", 339, 235, 110, (s, e) => { XoaForm(); LoadData(); }), "refresh");

        // ---- Thời tiết ----
        var gw = Tr(Ui.Grp(this, "", 500, 45, 488, 275), "grpWeather");
        btnWeather = Tr(Ui.Btn(gw, "", 15, 24, 150, btnWeather_Click), "weatherBtn");
        prg = new ProgressBar { Location = new Point(175, 29), Size = new Size(295, 20), Style = ProgressBarStyle.Marquee, Visible = false };
        gw.Controls.Add(prg);
        lblHead = new Label { Location = new Point(15, 65), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
        pnlResult = new Panel { Location = new Point(12, 90), Size = new Size(464, 175), BackColor = SystemColors.Control };
        lblIcon = new Label { Location = new Point(5, 5), Size = new Size(110, 90), Font = new Font("Segoe UI Emoji", 40F), TextAlign = ContentAlignment.MiddleCenter };
        lblDesc = new Label { Location = new Point(125, 8), Size = new Size(330, 32), Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
        lblTemp = new Label { Location = new Point(125, 45), Size = new Size(330, 22), Font = new Font("Segoe UI", 10.5F) };
        lblRain = new Label { Location = new Point(125, 70), Size = new Size(330, 22), Font = new Font("Segoe UI", 10.5F) };
        pnlAdvice = new Panel { Location = new Point(5, 112), Size = new Size(452, 52), BackColor = Color.Orange, Visible = false };
        lblAdvice = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.Black };
        pnlAdvice.Controls.Add(lblAdvice);
        pnlResult.Controls.AddRange(new Control[] { lblIcon, lblDesc, lblTemp, lblRain, pnlAdvice });
        gw.Controls.Add(lblHead); gw.Controls.Add(pnlResult);

        // ---- Tìm kiếm / lọc ----
        txtTim = Ui.Txt(this, 12, 330, 240);
        txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadData(); } };
        chkChuaHT = Tr(new CheckBox { AutoSize = true, Location = new Point(262, 332) }, "onlyUndone");
        Controls.Add(chkChuaHT);
        Tr(Ui.Btn(this, "", 560, 328, 90, (s, e) => LoadData()), "find");
        Tr(Ui.Btn(this, "", 658, 328, 110, (s, e) => BoLoc()), "clear");

        chkLocNgay = Tr(new CheckBox { AutoSize = true, Location = new Point(12, 366) }, "byDate");
        Controls.Add(chkLocNgay);
        Tr(Ui.Lbl(this, "", 200, 362), "from"); dtpTu = Ui.Dtp(this, 250, 362, 120);
        Tr(Ui.Lbl(this, "", 390, 362), "to");   dtpDen = Ui.Dtp(this, 440, 362, 120);
        dtpTu.Value = DateTime.Today; dtpDen.Value = DateTime.Today.AddDays(30);
        dtpTu.Enabled = dtpDen.Enabled = false;
        chkLocNgay.CheckedChanged += (s, e) => dtpTu.Enabled = dtpDen.Enabled = chkLocNgay.Checked;

        // ---- Lưới ----
        dgv = Ui.Grid(this, 12, 400, 976, 268);
        dgv.CellClick += Dgv_CellClick;
        dgv.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].DataPropertyName == nameof(TripRow.PhuongTien) && e.Value is string v)
            { e.Value = TransportText(v); e.FormattingApplied = true; }
        };
        Ui.Col(dgv, nameof(TripRow.MaChuyen), "", 50);
        Ui.Col(dgv, nameof(TripRow.TenChuyen), "", 200);
        Ui.Col(dgv, nameof(TripRow.DiaDiem), "", 150);
        Ui.Col(dgv, nameof(TripRow.NgayDi), "", 100, "d");
        Ui.Col(dgv, nameof(TripRow.SoNguoi), "", 80);
        Ui.Col(dgv, nameof(TripRow.NganSach), "", 130, "N0", right: true);
        Ui.Col(dgv, nameof(TripRow.PhuongTien), "", 100);
        dgv.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(TripRow.DaHoanThanh), Width = 90 });

        ApplyLanguage();
        Load += (s, e) => LoadData();
    }

    // ================= ĐA NGÔN NGỮ =================
    static string TransportText(string value)
    {
        foreach (var (v, k) in Transports) if (v == value) return T(k);
        return value;
    }

    void ApplyLanguage()
    {
        Text = T("title");
        foreach (var (c, k) in _tr) c.Text = T(k);
        txtTim.PlaceholderText = T("searchPh");

        string[] hk = { "colId", "colName", "colPlace", "colDate", "colPeople", "colBudget", "colTransport", "colDone" };
        for (int i = 0; i < hk.Length && i < dgv.Columns.Count; i++) dgv.Columns[i].HeaderText = T(hk[i]);
        foreach (DataGridViewColumn c in dgv.Columns) c.DefaultCellStyle.FormatProvider = Lang.Culture;

        string pattern = Lang.Culture.DateTimeFormat.ShortDatePattern;   // dd/MM/yyyy hoặc M/d/yyyy
        dtpNgayDi.CustomFormat = dtpTu.CustomFormat = dtpDen.CustomFormat = pattern;

        // nạp lại ComboBox phương tiện (giữ lựa chọn hiện tại)
        string? chon = (cboPT.SelectedItem as TItem)?.Value;
        var ds = Transports.Select(t => new TItem(t.Value, T(t.Key))).ToList();
        cboPT.DataSource = null;
        cboPT.DisplayMember = "Text"; cboPT.ValueMember = "Value";
        cboPT.DataSource = ds;
        cboPT.SelectedIndex = Math.Max(0, ds.FindIndex(x => x.Value == chon));

        dgv.Invalidate();
        _render?.Invoke();
    }

    // ================= DỮ LIỆU =================
    void LoadData()
    {
        try
        {
            using var db = AppDb.Create();
            IQueryable<ChuyenDi> q = db.ChuyenDis.AsNoTracking();
            string kw = txtTim.Text.Trim();
            if (kw.Length > 0) q = q.Where(x => x.TenChuyen.Contains(kw) || x.DiaDiem.Contains(kw));
            if (chkChuaHT.Checked) q = q.Where(x => !x.DaHoanThanh);
            if (chkLocNgay.Checked)
            {
                if (dtpTu.Value.Date > dtpDen.Value.Date) { Ui.Warn(T("m_range")); return; }
                var tu = DateOnly.FromDateTime(dtpTu.Value.Date);
                var den = DateOnly.FromDateTime(dtpDen.Value.Date);
                q = q.Where(x => x.NgayDi >= tu && x.NgayDi <= den);
            }
            var rows = q.OrderBy(x => x.NgayDi).ToList().Select(x => new TripRow
            {
                MaChuyen = x.MaChuyen, TenChuyen = x.TenChuyen, DiaDiem = x.DiaDiem,
                NgayDi = x.NgayDi.ToDateTime(TimeOnly.MinValue), SoNguoi = x.SoNguoi, NganSach = x.NganSach,
                PhuongTien = x.PhuongTien, DaHoanThanh = x.DaHoanThanh
            }).ToList();
            dgv.DataSource = rows;
            dgv.ClearSelection();
            _ma = null;
        }
        catch (Exception) { Ui.Error(T("m_dberr")); }
    }

    void BoLoc()
    {
        txtTim.Clear(); chkChuaHT.Checked = false; chkLocNgay.Checked = false;
        LoadData();
    }

    void Dgv_CellClick(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgv.Rows[e.RowIndex].DataBoundItem is not TripRow r) return;
        _ma = r.MaChuyen;
        txtTen.Text = r.TenChuyen; txtDiaDiem.Text = r.DiaDiem;
        dtpNgayDi.Value = r.NgayDi;
        nudSoNguoi.Value = Math.Clamp(r.SoNguoi, (int)nudSoNguoi.Minimum, (int)nudSoNguoi.Maximum);
        nudNganSach.Value = Math.Min(r.NganSach, nudNganSach.Maximum);
        cboPT.SelectedValue = r.PhuongTien;
        chkDone.Checked = r.DaHoanThanh;
    }

    bool HopLe(bool them)
    {
        if (string.IsNullOrWhiteSpace(txtTen.Text)) { Ui.Warn(T("v_name")); txtTen.Focus(); return false; }
        if (string.IsNullOrWhiteSpace(txtDiaDiem.Text)) { Ui.Warn(T("v_place")); txtDiaDiem.Focus(); return false; }
        if (them && dtpNgayDi.Value.Date < DateTime.Today) { Ui.Warn(T("v_date")); dtpNgayDi.Focus(); return false; }
        if (nudSoNguoi.Value < 1) { Ui.Warn(T("v_people")); return false; }
        if (nudNganSach.Value < 0) { Ui.Warn(T("v_budget")); return false; }
        return true;
    }

    void GanDuLieu(ChuyenDi c)
    {
        c.TenChuyen = txtTen.Text.Trim();
        c.DiaDiem = txtDiaDiem.Text.Trim();
        c.NgayDi = DateOnly.FromDateTime(dtpNgayDi.Value.Date);
        c.SoNguoi = (int)nudSoNguoi.Value;
        c.NganSach = nudNganSach.Value;
        c.PhuongTien = cboPT.SelectedValue as string ?? "Ô tô";
        c.DaHoanThanh = chkDone.Checked;
    }

    void btnThem_Click(object? s, EventArgs e)
    {
        if (!HopLe(true)) return;
        try
        {
            using var db = AppDb.Create();
            var c = new ChuyenDi(); GanDuLieu(c);
            db.ChuyenDis.Add(c); db.SaveChanges();
            XoaForm(); LoadData();
        }
        catch (Exception) { Ui.Error(T("m_dberr")); }
    }

    void btnSua_Click(object? s, EventArgs e)
    {
        if (_ma is null) { Ui.Warn(T("m_pickrow")); return; }
        if (!HopLe(false)) return;
        try
        {
            using var db = AppDb.Create();
            var c = db.ChuyenDis.Find(_ma.Value);
            if (c is null) { LoadData(); return; }
            GanDuLieu(c); db.SaveChanges();
            XoaForm(); LoadData();
        }
        catch (Exception) { Ui.Error(T("m_dberr")); }
    }

    void btnXoa_Click(object? s, EventArgs e)
    {
        if (_ma is null) { Ui.Warn(T("m_pickrow")); return; }
        if (MessageBox.Show(T("m_confirmDel", txtTen.Text), T("m_confirm"), MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        try
        {
            using var db = AppDb.Create();
            var c = db.ChuyenDis.Find(_ma.Value);
            if (c is not null) { db.ChuyenDis.Remove(c); db.SaveChanges(); }
            XoaForm(); LoadData();
        }
        catch (Exception) { Ui.Error(T("m_dberr")); }
    }

    void XoaForm()
    {
        _ma = null;
        txtTen.Clear(); txtDiaDiem.Clear(); dtpNgayDi.Value = DateTime.Today;
        nudSoNguoi.Value = 1; nudNganSach.Value = 0; cboPT.SelectedIndex = 0; chkDone.Checked = false;
        dgv.ClearSelection(); txtTen.Focus();
    }

    // ================= THỜI TIẾT =================
    static string Icon(int code) => code switch
    {
        0 => "☀️",
        >= 1 and <= 3 => "⛅",
        45 or 48 => "🌫️",
        (>= 51 and <= 67) or (>= 80 and <= 82) => "🌧️",
        >= 95 and <= 99 => "⛈️",
        (>= 71 and <= 77) or 85 or 86 => "❄️",
        _ => "🌡️"
    };

    static string Desc(int code) => code switch
    {
        0 => T("w_clear"),
        >= 1 and <= 3 => T("w_cloudy"),
        45 or 48 => T("w_fog"),
        (>= 51 and <= 67) or (>= 80 and <= 82) => T("w_rainy"),
        >= 95 and <= 99 => T("w_storm"),
        (>= 71 and <= 77) or 85 or 86 => T("w_snow"),
        _ => T("w_other")
    };

    void ShowMessage(string place, DateOnly date, string msg)
    {
        _render = () =>
        {
            lblHead.Text = T("w_head", date.ToString("d", Lang.Culture), place);
            lblIcon.Text = "❔"; lblDesc.Text = ""; lblTemp.Text = msg; lblRain.Text = "";
            lblTemp.Size = new Size(330, 70);
            pnlAdvice.Visible = false; pnlResult.BackColor = SystemColors.Control;
        };
        _render();
    }

    void ShowWeather(string place, DateOnly date, WeatherDay w)
    {
        _render = () =>
        {
            bool mua = (w.RainProb ?? 0) >= 60;
            lblHead.Text = T("w_head", date.ToString("d", Lang.Culture), place);
            lblIcon.Text = Icon(w.Code);
            lblDesc.Text = Desc(w.Code);
            lblTemp.Size = new Size(330, 22);
            lblTemp.Text = T("w_temp", Math.Round(w.TMin), Math.Round(w.TMax));
            lblRain.Text = w.RainProb is int p ? T("w_rain", p) : "";
            lblAdvice.Text = "⚠  " + T("w_advice");
            pnlAdvice.Visible = mua;                                   // cảnh báo khi xác suất mưa >= 60%
            pnlResult.BackColor = mua ? Color.FromArgb(255, 236, 205) : SystemColors.Control;
        };
        _render();
    }

    async void btnWeather_Click(object? s, EventArgs e)
    {
        string place = txtDiaDiem.Text.Trim();
        if (place.Length == 0) { Ui.Warn(T("w_pick")); return; }
        var date = DateOnly.FromDateTime(dtpNgayDi.Value.Date);
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Open-Meteo chỉ trả dự báo 16 ngày (hôm nay + 15 ngày)
        if (date < today || date > today.AddDays(15)) { ShowMessage(place, date, T("w_none")); return; }

        _cts?.Cancel(); _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        btnWeather.Enabled = false; prg.Visible = true;
        try
        {
            var geo = await WeatherService.GeocodeAsync(place, Lang.Code, ct);
            if (geo is null) { ShowMessage(place, date, T("w_notfound", place)); return; }
            var w = await WeatherService.ForecastAsync(geo.Lat, geo.Lon, date, ct);
            if (w is null) ShowMessage(place, date, T("w_none"));
            else ShowWeather(place, date, w);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException)
        {
            ShowMessage(place, date, T("w_net"));   // lỗi mạng / hết thời gian chờ 8 giây
        }
        finally { btnWeather.Enabled = true; prg.Visible = false; }
    }
}

public class TripRow
{
    public int MaChuyen { get; set; }
    public string TenChuyen { get; set; } = "";
    public string DiaDiem { get; set; } = "";
    public DateTime NgayDi { get; set; }
    public int SoNguoi { get; set; }
    public decimal NganSach { get; set; }
    public string PhuongTien { get; set; } = "";
    public bool DaHoanThanh { get; set; }
}
