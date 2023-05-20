using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace API.Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class ValidDob : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (!Regex.IsMatch((string) value, "^(?:[012]?[0-9]|3[01])[./-](?:0?[1-9]|1[0-2])[./-](?:[0-9]{2}){1,2}$")) 
            return new ValidationResult("Ngày tháng không hợp lệ");
        var date = DateTime.ParseExact((string) value, new []{"dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"}, CultureInfo.InvariantCulture);
        if (date > DateTime.Now)
        {
            return new ValidationResult("Không được sử dụng ngày trong tương lai");
        }
        return DateTime.Now.Year - date.Year >= 16 ? ValidationResult.Success : new ValidationResult("Chưa đủ 16 tuổi");
    }
}