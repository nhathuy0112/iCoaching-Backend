using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Status;

namespace Core.Entities;

public class Report : BaseEntity
{
    public int ContractId { get; set; }
    public Contract Contract { get; set; }
    public string Detail { get; set; }
    [Column(TypeName = "nvarchar(20)")]
    public ReportStatus ReportStatus { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? AdminId { get; set; }
    public AppUser Admin { get; set; }
    public string? SolutionDesc { get; set; }
    public string? RejectReason { get; set; }
    public ICollection<MediaAsset> MediaAssets { get; set; }
}         