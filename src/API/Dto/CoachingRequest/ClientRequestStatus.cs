using System.Text.Json.Serialization;

namespace API.Dto.CoachingRequest;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum ClientRequestStatus
{
    Init,
    Pending,
    Canceled,
    CoachRejected,
}