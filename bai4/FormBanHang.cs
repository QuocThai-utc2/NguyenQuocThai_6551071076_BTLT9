using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BanHangTraSua.Models;

namespace BanHangTraSua;

public record MonItem(int MaMon, string TenMon, decimal GiaBan)
{
    public override string ToString() => $"{TenMon} - {GiaBan.ToString("N0", new CultureInfo("vi-VN"))}";
}

public class CartItem
{
    public int MaMon { get; set; }
    public string TenMon { get; set; } = "";
    public int SoLy { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLy * DonGia;
}

public class FormBanHang : Form
{
    static readonly CultureInfo Vi = new("vi-VN");
    readonly BindingList<CartItem> _cart = new();
    int _qrTicket;

    ComboBox cboDM = null!;
    ListBox lstMon = null!;
    NumericUpDown nudSoLy = null!;
    DataGridView dgvCart = null!;
    Label lblTong = null!, lblThongTin = null!;
    PictureBox picQR = null!;
    Button btnCK = null!, btnTM = null!, btnDaNhan = null!;

    decimal Tong => _cart.Sum(c => c.ThanhTien);

    public FormBanHang()
    {
        Text = "Bán hàng - Quán trà sữa";
        ClientSize = new Size(1022, 600);
        MinimumSize = new Size(1040, 640);
        StartPosition = FormStartPosition.CenterScreen;

        // ----- Chọn món -----
        var g1 = Ui.Grp(this, "Chọn món", 12, 12, 270, 520);
        Ui.Lbl(g1, "Danh mục:", 15, 22);
        cboDM = Ui.Cbo(g1, 15, 48, 238);
        cboDM.SelectedIndexChanged += (s, e) => NapMon();
        lstMon = new ListBox { Location = new Point(15, 85), Size = new Size(238, 330), IntegralHeight = false };
        g1.Controls.Add(lstMon);
        Ui.Lbl(g1, "Số ly:", 15, 428);
        nudSoLy = Ui.Nud(g1, 80, 428, 80, 99, 1, 1);
        Ui.Btn(g1, "Thêm vào đơn", 15, 470, 238, btnThem_Click).Height = 34;

        // ----- Đơn hiện tại -----
        var g2 = Ui.Grp(this, "Đơn hiện tại", 292, 12, 420, 520);
        dgvCart = Ui.Grid(g2, 15, 25, 390, 385);
        Ui.Col(dgvCart, nameof(CartItem.TenMon), "Tên món", 150);
        Ui.Col(dgvCart, nameof(CartItem.SoLy), "SL", 40);
        Ui.Col(dgvCart, nameof(CartItem.DonGia), "Đơn giá", 80, "N0", right: true);
        Ui.Col(dgvCart, nameof(CartItem.ThanhTien), "Thành tiền", 100, "N0", right: true, fill: true);
        dgvCart.DataSource = _cart;
        Ui.Btn(g2, "Xóa dòng", 15, 420, 390, btnXoaDong_Click).Height = 32;
        lblTong = new Label
        {
            Location = new Point(15, 465), Size = new Size(390, 36), TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold), Text = "Tổng tiền: 0 đ"
        };
        g2.Controls.Add(lblTong);

        // ----- Thanh toán -----
        var g3 = Ui.Grp(this, "Thanh toán", 722, 12, 288, 520);
        btnCK = Ui.Btn(g3, "Thanh toán chuyển khoản", 15, 24, 258, btnCK_Click); btnCK.Height = 34;
        btnTM = Ui.Btn(g3, "Tiền mặt", 15, 64, 258, btnTM_Click); btnTM.Height = 34;
        picQR = new PictureBox { Location = new Point(34, 108), Size = new Size(220, 220), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
        g3.Controls.Add(picQR);
        lblThongTin = new Label { Location = new Point(15, 335), Size = new Size(258, 105), Font = new Font("Segoe UI", 9.5F) };
        g3.Controls.Add(lblThongTin);
        btnDaNhan = Ui.Btn(g3, "Đã nhận tiền", 15, 460, 258, btnDaNhan_Click);
        btnDaNhan.Height = 40; btnDaNhan.BackColor = Color.SeaGreen; btnDaNhan.ForeColor = Color.White;
        btnDaNhan.UseVisualStyleBackColor = false; btnDaNhan.Enabled = false;

        Ui.Btn(this, "Quản lý thực đơn", 12, 550, 170, (s, e) => { using var f = new FormThucDon(); f.ShowDialog(this); NapDanhMuc(); });
        Ui.Btn(this, "Danh sách hóa đơn", 190, 550, 170, (s, e) => { using var f = new FormHoaDon(); f.ShowDialog(this); });

        Load += (s, e) => NapDanhMuc();
    }

    // ================= DANH MỤC / MÓN =================
    void NapDanhMuc()
    {
        try
        {
            using var db = AppDb.Create();
            var ds = db.DanhMucMons.AsNoTracking().OrderBy(d => d.MaDM).ToList();
            cboDM.DisplayMember = "TenDM"; cboDM.ValueMember = "MaDM";
            cboDM.DataSource = ds;
            NapMon();
        }
        catch (Exception ex) { BaoLoiDb(ex, "nạp danh mục"); }
    }

    void NapMon()
    {
        if (cboDM.SelectedItem is not DanhMucMon dm) { lstMon.DataSource = null; return; }
        try
        {
            using var db = AppDb.Create();
            var ds = db.MonUongs.AsNoTracking().Where(m => m.MaDM == dm.MaDM && m.DangBan)
                .OrderBy(m => m.TenMon).ToList()
                .Select(m => new MonItem(m.MaMon, m.TenMon, m.GiaBan)).ToList();
            lstMon.DataSource = ds;
        }
        catch (Exception ex) { BaoLoiDb(ex, "nạp danh sách món"); }
    }

    // ================= GIỎ HÀNG =================
    void btnThem_Click(object? s, EventArgs e)
    {
        if (lstMon.SelectedItem is not MonItem m) { Ui.Warn("Hãy chọn một món trong danh sách."); return; }
        int n = (int)nudSoLy.Value;
        var co = _cart.FirstOrDefault(c => c.MaMon == m.MaMon);
        if (co != null) { co.SoLy += n; _cart.ResetBindings(); }          // cộng dồn số lượng
        else _cart.Add(new CartItem { MaMon = m.MaMon, TenMon = m.TenMon, SoLy = n, DonGia = m.GiaBan });
        nudSoLy.Value = 1;
        CapNhatTong();
    }

    void btnXoaDong_Click(object? s, EventArgs e)
    {
        if (dgvCart.CurrentRow?.DataBoundItem is CartItem c) { _cart.Remove(c); CapNhatTong(); }
        else Ui.Warn("Hãy chọn một dòng trong đơn để xóa.");
    }

    void CapNhatTong()
    {
        lblTong.Text = $"Tổng tiền: {Tong.ToString("N0", Vi)} đ";
        ResetThanhToan();   // giỏ hàng đổi -> mã QR cũ không còn đúng số tiền
    }

    void ResetThanhToan()
    {
        _qrTicket++;
        picQR.Image?.Dispose(); picQR.Image = null;
        lblThongTin.Text = "";
        btnDaNhan.Enabled = false;
    }

    // ================= THANH TOÁN =================
    async void btnCK_Click(object? s, EventArgs e)
    {
        if (_cart.Count == 0) { Ui.Warn("Đơn hàng đang trống."); return; }
        btnCK.Enabled = false;
        int ticket = ++_qrTicket;
        int maDuKien = 0;
        try
        {
            using (var db = AppDb.Create()) maDuKien = (db.HoaDons.Max(h => (int?)h.MaHD) ?? 0) + 1;
            var pay = AppConfig.Load().Payment;            // đọc lại appsettings.json mỗi lần tạo QR
            decimal tong = Tong;
            string noiDung = $"TRASUA HD{maDuKien}";

            // Luôn hiện thông tin dạng chữ để khách chuyển tay nếu QR không tải được
            lblThongTin.Text = $"Chủ TK: {pay.AccountName}\nNgân hàng: {pay.BankId}\nSTK: {pay.AccountNo}\n" +
                               $"Số tiền: {tong.ToString("N0", Vi)} đ\nNội dung: {noiDung}";
            btnDaNhan.Enabled = true;
            picQR.Image?.Dispose(); picQR.Image = null;
            Log.Information("Tạo QR cho HD{MaHD}, số tiền {SoTien}, tài khoản {TaiKhoan}", maDuKien, tong, VietQrService.Mask(pay.AccountNo));

            var img = await VietQrService.LoadAsync(VietQrService.BuildUrl(pay, tong, noiDung));
            if (ticket != _qrTicket) { img.Dispose(); return; }   // giỏ hàng đã đổi trong lúc tải
            picQR.Image = img;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or ArgumentException or ExternalException)
        {
            Log.Error(ex, "Không tải được ảnh QR cho HD{MaHD}", maDuKien);
            if (ticket == _qrTicket)
            {
                lblThongTin.Text += "\n(Không có mã QR - chuyển khoản tay theo thông tin trên)";
                Ui.Warn("Không tải được mã QR (có thể mất mạng).\nKhách có thể chuyển khoản tay theo thông tin hiển thị,\nhoặc bạn chọn thanh toán Tiền mặt.");
            }
        }
        catch (Exception ex) { BaoLoiDb(ex, "chuẩn bị thanh toán"); }
        finally { btnCK.Enabled = true; }
    }

    void btnDaNhan_Click(object? s, EventArgs e)
    {
        if (MessageBox.Show("Bạn đã kiểm tra tin nhắn ngân hàng và đã nhận đủ tiền chưa?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        LuuHoaDon("Chuyển khoản");
    }

    void btnTM_Click(object? s, EventArgs e)
    {
        if (_cart.Count == 0) { Ui.Warn("Đơn hàng đang trống."); return; }
        if (MessageBox.Show($"Thu tiền mặt {Tong.ToString("N0", Vi)} đ?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        LuuHoaDon("Tiền mặt");
    }

    void LuuHoaDon(string hinhThuc)
    {
        if (_cart.Count == 0) return;
        try
        {
            using var db = AppDb.Create();
            var hd = new HoaDon { NgayLap = DateTime.Now, TongTien = Tong, HinhThucTT = hinhThuc, DaThanhToan = true };
            foreach (var c in _cart)
                hd.ChiTietHoaDons.Add(new ChiTietHoaDon { MaMon = c.MaMon, SoLy = c.SoLy, DonGia = c.DonGia });
            db.HoaDons.Add(hd);
            db.SaveChanges();   // HoaDon + ChiTietHoaDon trong cùng một lần SaveChanges
            Log.Information("Lưu HD{MaHD}, {HinhThuc}, tổng {TongTien}, {SoMon} dòng", hd.MaHD, hinhThuc, hd.TongTien, hd.ChiTietHoaDons.Count);

            _cart.Clear(); CapNhatTong();
            Ui.Info($"Đã lưu hóa đơn HD{hd.MaHD} ({hinhThuc}).");
        }
        catch (Exception ex) { BaoLoiDb(ex, "lưu hóa đơn"); }
    }

    static void BaoLoiDb(Exception ex, string hanhDong)
    {
        Log.Error(ex, "Lỗi truy cập CSDL khi {HanhDong}", hanhDong);
        Ui.Error("Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra kết nối rồi thử lại.\n(Chi tiết lỗi đã được ghi vào thư mục Logs.)");
    }
}
