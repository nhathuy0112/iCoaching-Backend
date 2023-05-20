using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace API.Attribute;

public class ValidTrainingLogDate : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (!Regex.IsMatch((string) value, "^(?:[012]?[0-9]|3[01])[./-](?:0?[1-9]|1[0-2])[./-](?:[0-9]{2}){1,2}$")) 
            return new ValidationResult("Ngày tháng không hợp lệ");
        var date = DateTime.ParseExact((string) value, new []{"dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"}, CultureInfo.InvariantCulture);
        if (date > DateTime.Now.Date)
        {
            return new ValidationResult("Không được sử dụng ngày trong tương lai");
        }
        return ValidationResult.Success;
    }
}