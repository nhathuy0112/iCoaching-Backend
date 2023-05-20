using System.Text.Json.Serialization;

namespace API.Dto.Contract.Status;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminContractStatusOption
{
    Cancel,
    End
}