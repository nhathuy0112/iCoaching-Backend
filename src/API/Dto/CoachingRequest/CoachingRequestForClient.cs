using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Dto.CoachingRequest;

public class CoachingRequestForClient : CoachingRequestBase
{
    public string? Discount { get; set; }
    public string? PriceToPay { get; set; }
    public string CoachName { get; set; }
}