using System.ComponentModel.DataAnnotations;

namespace API.Dto.Auth;

public class LogoutRequest
{
    [Required]
    public string CurrentRefreshToken { get; set; }
}