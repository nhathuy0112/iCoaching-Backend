using System.Text.Json.Serialization;

namespace Core.Entities.Status;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CertStatus
{
    Pending,
    Accepted,
    Denied
}