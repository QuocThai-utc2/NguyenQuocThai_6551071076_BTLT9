using System.Globalization;
using System.Text.Json;

namespace KeHoachDaNgoai;

public static class Lang
{
    public static string Code { get; private set; } = "vi";
    public static CultureInfo Culture { get; private set; } = new("vi-VN");

    static readonly Dictionary<string, (string Vi, string En)> D = new()
    {
        ["title"] = ("Kế hoạch dã ngoại", "Picnic Trip Planner"),
        ["lang"] = ("Ngôn ngữ", "Language"),
        ["grpInfo"] = ("Thông tin chuyến đi", "Trip information"),
        ["grpWeather"] = ("Thời tiết", "Weather"),
        ["name"] = ("Tên chuyến", "Trip name"),
        ["place"] = ("Địa điểm", "Destination"),
        ["date"] = ("Ngày đi", "Date"),
        ["people"] = ("Số người", "People"),
        ["budget"] = ("Ngân sách", "Budget"),
        ["transport"] = ("Phương tiện", "Transport"),
        ["done"] = ("Đã hoàn thành", "Completed"),
        ["add"] = ("Thêm", "Add"),
        ["edit"] = ("Sửa", "Edit"),
        ["del"] = ("Xóa", "Delete"),
        ["refresh"] = ("Làm mới", "Refresh"),
        ["searchPh"] = ("Tìm tên chuyến hoặc địa điểm...", "Search trip or destination..."),
        ["onlyUndone"] = ("Chỉ hiện chuyến chưa hoàn thành", "Show incomplete trips only"),
        ["byDate"] = ("Lọc theo ngày đi", "Filter by date"),
        ["from"] = ("Từ", "From"),
        ["to"] = ("Đến", "To"),
        ["find"] = ("Tìm", "Search"),
        ["clear"] = ("Bỏ lọc", "Clear filter"),
        ["weatherBtn"] = ("Xem thời tiết", "Check weather"),
        ["colId"] = ("Mã", "ID"),
        ["colName"] = ("Tên chuyến", "Trip name"),
        ["colPlace"] = ("Địa điểm", "Destination"),
        ["colDate"] = ("Ngày đi", "Date"),
        ["colPeople"] = ("Số người", "People"),
        ["colBudget"] = ("Ngân sách (VND)", "Budget (VND)"),
        ["colTransport"] = ("Phương tiện", "Transport"),
        ["colDone"] = ("Hoàn thành", "Done"),
        ["t_moto"] = ("Xe máy", "Motorbike"),
        ["t_car"] = ("Ô tô", "Car"),
        ["t_coach"] = ("Xe khách", "Coach"),
        ["t_train"] = ("Tàu hỏa", "Train"),
        ["w_head"] = ("Dự báo thời tiết ngày {0} - {1}", "Weather forecast for {0} - {1}"),
        ["w_temp"] = ("Nhiệt độ: {0}°C - {1}°C", "Temperature: {0}°C - {1}°C"),
        ["w_rain"] = ("Khả năng mưa: {0}%", "Chance of rain: {0}%"),
        ["w_advice"] = ("Nên mang áo mưa hoặc cân nhắc dời lịch", "Bring a raincoat or consider rescheduling"),
        ["w_none"] = ("Chưa có dữ liệu dự báo", "No forecast data available yet"),
        ["w_notfound"] = ("Không tìm thấy địa danh \"{0}\". Hãy thử tên không dấu, ví dụ \"Da Lat\".", "Place \"{0}\" was not found. Try the name without diacritics, e.g. \"Da Lat\"."),
        ["w_net"] = ("Không tải được dự báo, vui lòng kiểm tra kết nối mạng.", "Could not load the forecast. Please check your network connection."),
        ["w_pick"] = ("Hãy chọn một chuyến đi trong danh sách (hoặc nhập địa điểm).", "Please select a trip from the list (or enter a destination)."),
        ["w_clear"] = ("Trời quang", "Clear sky"),
        ["w_cloudy"] = ("Có mây", "Partly cloudy"),
        ["w_fog"] = ("Sương mù", "Fog"),
        ["w_rainy"] = ("Mưa", "Rain"),
        ["w_storm"] = ("Dông", "Thunderstorm"),
        ["w_snow"] = ("Tuyết", "Snow"),
        ["w_other"] = ("Không xác định", "Unknown"),
        ["v_name"] = ("Tên chuyến không được để trống.", "Trip name is required."),
        ["v_place"] = ("Địa điểm không được để trống.", "Destination is required."),
        ["v_date"] = ("Ngày đi không được nhỏ hơn hôm nay.", "The date cannot be earlier than today."),
        ["v_people"] = ("Số người phải từ 1 trở lên.", "People must be at least 1."),
        ["v_budget"] = ("Ngân sách phải từ 0 trở lên.", "Budget must be 0 or more."),
        ["m_pickrow"] = ("Hãy bấm chọn một dòng trong danh sách trước.", "Please select a row in the list first."),
        ["m_confirmDel"] = ("Xóa chuyến \"{0}\"?", "Delete trip \"{0}\"?"),
        ["m_confirm"] = ("Xác nhận", "Confirm"),
        ["m_notice"] = ("Thông báo", "Notice"),
        ["m_dberr"] = ("Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra kết nối.", "Cannot access the database. Please check the connection."),
        ["m_range"] = ("\"Từ\" không được lớn hơn \"Đến\".", "\"From\" cannot be later than \"To\"."),
    };

    public static string T(string key) => D.TryGetValue(key, out var v) ? (Code == "en" ? v.En : v.Vi) : key;
    public static string T(string key, params object[] args) => string.Format(Culture, T(key), args);

    public static void Set(string code)
    {
        Code = code == "en" ? "en" : "vi";
        Culture = new CultureInfo(Code == "en" ? "en-US" : "vi-VN");
        CultureInfo.CurrentCulture = Culture;
        CultureInfo.CurrentUICulture = Culture;
        CultureInfo.DefaultThreadCurrentCulture = Culture;
        CultureInfo.DefaultThreadCurrentUICulture = Culture;
    }

    static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeHoachDaNgoai", "usersettings.json");

    public static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(SettingsPath));
                if (doc.RootElement.TryGetProperty("Language", out var l)) { Set(l.GetString() ?? "vi"); return; }
            }
        }
        catch { /* file hỏng -> dùng mặc định */ }
        Set("vi");
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new { Language = Code }));
        }
        catch { }
    }
}
