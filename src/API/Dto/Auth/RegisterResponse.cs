using System.Text.Json.Serialization;

namespace API.Dto.Auth;

public class RegisterResponse
{
    public string Email { get; set; }
    public string Username { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }
    public string Role { get; set; }
}