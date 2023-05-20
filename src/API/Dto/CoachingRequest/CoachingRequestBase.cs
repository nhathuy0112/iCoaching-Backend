namespace API.Dto.CoachingRequest;

public class CoachingRequestBase
{
    public int Id { get; set; }
    public string Status { get; set; }
    public string CourseName { get; set; }
    public string Price { get; set; }
    public int Duration { get; set; }
    public string? ClientMessage { get; set; }
    public string? CancelReason { get; set; }
    public string? RejectReason { get; set; }

}