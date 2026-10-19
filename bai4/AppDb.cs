using Microsoft.EntityFrameworkCore;
using BanHangTraSua.Models;

namespace BanHangTraSua;

public static class AppDb
{
    public static QlTraSuaContext Create()
    {
        var options = new DbContextOptionsBuilder<QlTraSuaContext>()
            .UseSqlServer(AppConfig.Settings.Database.ConnectionString)
            .Options;
        return new QlTraSuaContext(options);
    }
}
