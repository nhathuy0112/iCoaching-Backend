using System.Text.Json.Serialization;

namespace API.Dto.Contract.Report;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportOptionForAdmin
{
    Solve,
    Reject
}