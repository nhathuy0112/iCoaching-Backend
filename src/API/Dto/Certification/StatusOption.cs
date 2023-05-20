using System.Text.Json.Serialization;

namespace API.Dto.Certification;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusOption
{
    Accepted,
    Denied
}