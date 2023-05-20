using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Dto.User;

namespace API.Dto.Auth;

public class RegisterRequest : BaseUserProfile
{
    [Required(ErrorMessage = "Tên người dùng không được phép để trống")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "Mật khẩu không được phép để trống")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$",
        ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ cái in hoa, 1 chữ cái in thường, 1 số, 1 kí tự đặc biệt và độ dài ít nhất là 6 kí tự")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Mật khẩu không được để trống"), Compare("Password", ErrorMessage = "Mật khẩu xác nhận không đúng")]
    public string ConfirmPassword { get; set; }

    public bool IsCoach { get; set; } = false;
}