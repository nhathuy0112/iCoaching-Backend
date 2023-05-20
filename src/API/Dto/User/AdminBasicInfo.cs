using System.Configuration;
using System.Text.Json.Serialization;

namespace API.Dto.User;

public class AdminBasicInfo : BaseUserProfile
{
    public string Id { get; set; }
    public string UserName { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Dob { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Gender { get; set; }

    public bool IsLocked { get; set; }
    public string AvatarUrl { get; set; }
    public string? Note { get; set; }
}