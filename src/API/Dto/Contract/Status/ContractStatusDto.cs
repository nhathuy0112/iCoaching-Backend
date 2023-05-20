using System.Text.Json.Serialization;

namespace API.Dto.Contract.Status;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContractStatusDto
{
    Active,
    Pending,
    Complete,
    Canceled
}