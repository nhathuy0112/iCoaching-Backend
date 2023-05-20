using System.Globalization;

namespace API.Helpers;

public static class CurrencyHelper
{
    public static string GetVnd(long money)
    {
        var cul = CultureInfo.GetCultureInfo("vi-VN");
        if (money == 0) return "0 VNĐ";
        return money.ToString("#,###", cul.NumberFormat).Replace(".", ",") + " VNĐ";
    }
}