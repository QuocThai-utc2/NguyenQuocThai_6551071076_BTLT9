using System.Globalization;
using System.Text;
using Serilog;

namespace QuanLyNhaTro;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        var vi = new CultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentCulture = vi;
        CultureInfo.DefaultThreadCurrentUICulture = vi;
        CultureInfo.CurrentCulture = vi;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30,
                encoding: new UTF8Encoding(true))
            .CreateLogger();

        // Xử lý lỗi toàn cục
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => XuLy(e.Exception, "ThreadException");
        AppDomain.CurrentDomain.UnhandledException += (_, e) => XuLy(e.ExceptionObject as Exception, "UnhandledException");

        try
        {
            Log.Information("Ứng dụng khởi động");
            _ = AppConfig.Settings;
            ApplicationConfiguration.Initialize();
            Application.Run(new FormKhachThue());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Lỗi khi khởi động");
            MessageBox.Show("Không thể khởi động ứng dụng. Vui lòng kiểm tra file appsettings.json.\nChi tiết đã được ghi vào thư mục Logs.",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { Log.CloseAndFlush(); }
    }

    static void XuLy(Exception? ex, string nguon)
    {
        Log.Error(ex, "Lỗi chưa được bắt ({Nguon})", nguon);   // ghi cả stack trace
        MessageBox.Show("Đã xảy ra lỗi, thông tin đã được ghi lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
