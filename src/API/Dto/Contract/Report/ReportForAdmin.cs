namespace API.Dto.Contract.Report;

public class ReportForAdmin : ReportBaseDto
{
    public int ContractId { get; set; }
    public string ClientFullName { get; set; }
    public string ClientEmail { get; set; }
}