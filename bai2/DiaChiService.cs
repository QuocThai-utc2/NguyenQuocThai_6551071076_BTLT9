using System.Text.Json;

namespace QuanLyNhaTro;

public record DiaChi(int Code, string Name)
{
    public override string ToString() => Name;
}

public static class DiaChiService
{
    // HttpClient dùng chung cho toàn ứng dụng, timeout 10 giây
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    const string Base = "https://provinces.open-api.vn/api/v2/";

    public static async Task<List<DiaChi>> LayTinhAsync()
    {
        using var stream = await Http.GetStreamAsync(Base);
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.EnumerateArray()
            .Select(e => new DiaChi(e.GetProperty("code").GetInt32(), e.GetProperty("name").GetString() ?? ""))
            .ToList();
    }

    public static async Task<List<DiaChi>> LayPhuongXaAsync(int maTinh)
    {
        using var stream = await Http.GetStreamAsync($"{Base}p/{maTinh}?depth=2");
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("wards").EnumerateArray()
            .Select(e => new DiaChi(e.GetProperty("code").GetInt32(), e.GetProperty("name").GetString() ?? ""))
            .ToList();
    }
}
