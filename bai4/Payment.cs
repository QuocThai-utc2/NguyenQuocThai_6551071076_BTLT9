namespace BanHangTraSua;

// Mục "Payment" trong appsettings.json (lớp cấu hình có kiểu, không hard-code)
public class PaymentSettings
{
    public string ShopName { get; set; } = "";
    public string BankId { get; set; } = "";
    public string AccountNo { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string Template { get; set; } = "compact2";
}

public partial class AppSettings
{
    public PaymentSettings Payment { get; set; } = new();
}
