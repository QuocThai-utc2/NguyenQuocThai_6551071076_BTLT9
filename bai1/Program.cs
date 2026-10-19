using System.Globalization;
using System.Text;
using Serilog;

namespace SoChiTieu;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        var vi = new CultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentCulture = vi;
        CultureInfo.DefaultThreadCurrentUICulture = vi;
        CultureInfo.CurrentCulture = vi;

        // Logs nằm cạnh file .exe, mỗi ngày một file: log-yyyyMMdd.txt
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30,
                encoding: new UTF8Encoding(true))
            .CreateLogger();

        try
        {
            Log.Information("Ứng dụng khởi động");
            _ = AppConfig.Settings; // đọc cấu hình sớm để báo lỗi rõ ràng
            ApplicationConfiguration.Initialize();
            Application.Run(new FormChiTieu());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Ứng dụng gặp lỗi nghiêm trọng khi khởi động");
            MessageBox.Show("Không thể khởi động ứng dụng. Vui lòng kiểm tra file appsettings.json.\nChi tiết đã được ghi vào thư mục Logs.",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.Information("Ứng dụng đóng");
            Log.CloseAndFlush();
        }
    }
}
