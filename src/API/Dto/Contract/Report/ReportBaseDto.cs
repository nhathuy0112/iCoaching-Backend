namespace API.Dto.Contract.Report;

public class ReportBaseDto
{
    public int Id { get; set; }
    public string ReportStatus { get; set; }
    public string CreatedDate { get; set; }
    public List<string> Images { get; set; }
    public string Detail { get; set; }
    public string? SolutionDesc { get; set; }
    public string? RejectReason { get; set; }
}