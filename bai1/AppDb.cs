using Microsoft.EntityFrameworkCore;
using SoChiTieu.Models;

namespace SoChiTieu;

public static class AppDb
{
    public static QlChiTieuContext Create()
    {
        var options = new DbContextOptionsBuilder<QlChiTieuContext>()
            .UseSqlServer(AppConfig.Settings.Database.ConnectionString)
            .Options;
        return new QlChiTieuContext(options);
    }
}
