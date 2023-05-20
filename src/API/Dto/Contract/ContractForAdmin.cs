namespace API.Dto.Contract;

public class ContractForAdmin
{
    public int Id { get; set; }
    public string ClientName { get; set; }
    public string ClientGender { get; set; }
    public int ClientAge { get; set; }
    public string ClientEmail { get; set; }
    public string ClientPhoneNumber { get; set; }
    public string CourseName { get; set; }
    public int Duration { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
}