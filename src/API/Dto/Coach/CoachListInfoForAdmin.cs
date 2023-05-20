using System.Text.Json.Serialization;
using API.Dto.User;

namespace API.Dto.Coach;

public class CoachListInfoForAdmin : BaseUserProfile
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public bool IsVerified { get; set; }
    public bool IsLocked { get; set; }
    public int Age { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Dob { get; set; }

    public int WarningCount { get; set; }
    public string AvatarUrl { get; set; }
}