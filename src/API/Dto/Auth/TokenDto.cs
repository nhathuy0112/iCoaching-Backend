using System.ComponentModel.DataAnnotations;

namespace API.Dto.Auth;

public class TokenDto
{
    [Required]
    public string AccessToken { get; set; }
    [Required]
    public string RefreshToken { get; set; }
}