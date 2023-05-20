using System.Text.Json.Serialization;

namespace API.Dto.Certification;

public class CertRequestDetailForCoach
{
    public int Id { get; set; }
    public string Status { get; set; }
    public List<string> CertUrls { get; set; }
    public List<string> IdUrls { get; set; }
    public string Reason { get; set; }
}