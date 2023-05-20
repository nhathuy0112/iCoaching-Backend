namespace API.Dto.Contract;

public class ContractForClient
{
    public int Id { get; set; }
    public string CoachName { get; set; }
    public string CoachEmail { get; set; }
    public string CoachPhoneNumber { get; set; }
    public string CoachGender { get; set; }
    public string CourseName { get; set; }
    public int Duration { get; set; }
    public string? CancelReason { get; set; }

}