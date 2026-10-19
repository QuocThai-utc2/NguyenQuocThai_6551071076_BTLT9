namespace BanHangTraSua;

public static class VietQrService
{
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static string BuildUrl(PaymentSettings p, decimal amount, string noiDung) =>
        $"https://img.vietqr.io/image/{Uri.EscapeDataString(p.BankId)}-{Uri.EscapeDataString(p.AccountNo)}-{Uri.EscapeDataString(p.Template)}.png" +
        $"?amount={Uri.EscapeDataString(((long)amount).ToString())}" +
        $"&addInfo={Uri.EscapeDataString(noiDung)}" +
        $"&accountName={Uri.EscapeDataString(p.AccountName)}";

    public static async Task<Image> LoadAsync(string url)
    {
        byte[] bytes = await Http.GetByteArrayAsync(url);
        using var ms = new MemoryStream(bytes);
        using var img = Image.FromStream(ms);
        return new Bitmap(img);   // bản sao độc lập, không phụ thuộc MemoryStream
    }

    // Chỉ ghi vài số cuối của tài khoản vào log
    public static string Mask(string acc) => acc.Length <= 3 ? "***" : new string('*', acc.Length - 3) + acc[^3..];
}
