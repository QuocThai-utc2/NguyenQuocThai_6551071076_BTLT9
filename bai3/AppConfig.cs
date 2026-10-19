using Microsoft.Extensions.Configuration;

namespace KeHoachDaNgoai;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public partial class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
}

public static class AppConfig
{
    public static AppSettings Settings { get; } = Load();

    // Đọc lại file mỗi lần gọi Load() (dùng cho cấu hình cần cập nhật nóng)
    public static AppSettings Load()
    {
#if DEBUG
        const string env = "Development";
#else
        const string env = "Production";
#endif
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .Build();

        var s = config.Get<AppSettings>() ?? new AppSettings();
        if (string.IsNullOrWhiteSpace(s.Database.ConnectionString))
            throw new InvalidOperationException("Thiếu Database:ConnectionString trong appsettings.json");
        return s;
    }
}
