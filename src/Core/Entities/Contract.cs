using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Status;

namespace Core.Entities;

public class Contract : BaseEntity
{
    public string CoachId { get; set; }
    public AppUser Coach { get; set; }
    public string ClientId { get; set; }
    public AppUser Client { get; set; }
    [Column(TypeName = "nvarchar(20)")]
    public ContractStatus Status { get; set; }
    public string CourseName { get; set; }
    public DateTime CreatedDate { get; set; }
    public int Duration { get; set; }
    public string? CourseDescription { get; set; }
    public long Price { get; set; }
    public string? Description { get; set; }
    public string? CancelReason { get; set; }
    public string? CreatedBy { get; set; }
    public string? BackgroundJobId { get; set; }
    public ICollection<FileAsset> Files { get; set; }
    public ICollection<TrainingLog> Logs { get; set; }
    public ICollection<Report> Reports { get; set; }
}