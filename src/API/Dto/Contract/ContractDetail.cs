namespace API.Dto.Contract;

public class UserInfo
{
    public string Id { get; set; }
    public string Fullname { get; set; }
    public string Gender { get; set; }
    public int Age{ get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
}

public class ContractDetail
{
    public UserInfo Client { get; set; }
    public UserInfo Coach { get; set; }
    public string CourseName { get; set; }
    public string Status { get; set; }
    public string CreatedDate { get; set; }
    public int Duration { get; set; }
    public string Price { get; set; }
    public bool IsReported { get; set; }
    public bool IsComplete { get; set; }
    public string? CourseDescription { get; set; }
    public string? Description { get; set; }
    public string? CancelReason { get; set; }
}