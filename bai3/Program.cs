namespace KeHoachDaNgoai;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Lang.Load();   // đọc ngôn ngữ đã lưu trong %LocalAppData%\KeHoachDaNgoai\usersettings.json
        try
        {
            _ = AppConfig.Settings;
            ApplicationConfiguration.Initialize();
            Application.Run(new FormChuyenDi());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể khởi động ứng dụng. Vui lòng kiểm tra file appsettings.json.\n" + ex.Message,
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
