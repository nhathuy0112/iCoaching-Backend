using System.ComponentModel.DataAnnotations;

namespace API.Dto.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Tài khoản không được phép để trống")]
    public string Username { get; set; }
    [Required(ErrorMessage = "Mật khẩu không được phép để trống")]
    public string Password { get; set; }
}