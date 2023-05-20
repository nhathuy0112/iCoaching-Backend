namespace API.Dto.CoachingRequest;

public class CoachingRequestForCoach : CoachingRequestBase
{
    public string ClientName { get; set; }
    public string Gender { get; set; }
    public string Age { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}