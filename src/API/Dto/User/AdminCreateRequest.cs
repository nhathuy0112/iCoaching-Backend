using System.Text.Json.Serialization;
using API.Dto.Auth;

namespace API.Dto.User;

public class AdminCreateRequest : RegisterRequest
{
    public string? Note { get; set; }
    [JsonIgnore]
    public string? Dob { get; set; }
    [JsonIgnore]
    public string? Gender { get; set; }
    [JsonIgnore] 
    public bool IsCoach { get; set; }
}