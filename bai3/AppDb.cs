using Microsoft.EntityFrameworkCore;
using KeHoachDaNgoai.Models;

namespace KeHoachDaNgoai;

public static class AppDb
{
    public static QlDaNgoaiContext Create()
    {
        var options = new DbContextOptionsBuilder<QlDaNgoaiContext>()
            .UseSqlServer(AppConfig.Settings.Database.ConnectionString)
            .Options;
        return new QlDaNgoaiContext(options);
    }
}
