using System.Text.Json.Serialization;

namespace Core.Entities.Status;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportStatus
{
    Pending,
    Solved,
    Rejected
}