using System.Text.Json.Serialization;

namespace API.Dto.CoachingRequest;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoachingRequestOption
{
    Accept,
    Reject
}