using Microsoft.EntityFrameworkCore;
using QuanLyNhaTro.Models;

namespace QuanLyNhaTro;

public static class AppDb
{
    public static QlNhaTroContext Create()
    {
        var options = new DbContextOptionsBuilder<QlNhaTroContext>()
            .UseSqlServer(AppConfig.Settings.Database.ConnectionString)
            .Options;
        return new QlNhaTroContext(options);
    }
}
