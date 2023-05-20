using System.Text.Json.Serialization;

namespace API.Dto.Coach;

public class TrainingCourseResponse : TrainingCourseBaseDto
{
    public int Id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsClientRequested { get; set; }
    public string Price { get; set; }
}