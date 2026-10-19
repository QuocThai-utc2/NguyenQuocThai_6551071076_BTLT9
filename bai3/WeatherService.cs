using System.Globalization;
using System.Text.Json;

namespace KeHoachDaNgoai;

public record GeoResult(double Lat, double Lon, string Name);
public record WeatherDay(int Code, double TMin, double TMax, int? RainProb);

public static class WeatherService
{
    // HttpClient dùng chung, timeout 8 giây
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    // Lưu ý: tên miền đúng của Open-Meteo là open-meteo.com (có dấu gạch ngang)
    public static async Task<GeoResult?> GeocodeAsync(string place, string lang, CancellationToken ct)
    {
        string url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(place)}&count=1&language={lang}";
        using var stream = await Http.GetStreamAsync(url, ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        if (!doc.RootElement.TryGetProperty("results", out var rs) || rs.GetArrayLength() == 0) return null;
        var r = rs[0];
        return new GeoResult(r.GetProperty("latitude").GetDouble(), r.GetProperty("longitude").GetDouble(),
            r.TryGetProperty("name", out var n) ? n.GetString() ?? place : place);
    }

    public static async Task<WeatherDay?> ForecastAsync(double lat, double lon, DateOnly date, CancellationToken ct)
    {
        var inv = CultureInfo.InvariantCulture;
        string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(inv)}&longitude={lon.ToString(inv)}" +
                     "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto&forecast_days=16";
        using var stream = await Http.GetStreamAsync(url, ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var daily = doc.RootElement.GetProperty("daily");

        string key = date.ToString("yyyy-MM-dd", inv);
        int idx = -1, i = 0;
        foreach (var t in daily.GetProperty("time").EnumerateArray())
        {
            if (t.GetString() == key) { idx = i; break; }
            i++;
        }
        if (idx < 0) return null;

        var rainEl = daily.GetProperty("precipitation_probability_max")[idx];
        var codeEl = daily.GetProperty("weather_code")[idx];
        return new WeatherDay(
            codeEl.ValueKind == JsonValueKind.Null ? -1 : codeEl.GetInt32(),
            daily.GetProperty("temperature_2m_min")[idx].GetDouble(),
            daily.GetProperty("temperature_2m_max")[idx].GetDouble(),
            rainEl.ValueKind == JsonValueKind.Null ? null : (int)Math.Round(rainEl.GetDouble()));
    }
}
