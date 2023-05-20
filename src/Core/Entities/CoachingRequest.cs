using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Auth;
using Core.Entities.Status;

namespace Core.Entities;

public class CoachingRequest : BaseEntity
{
    [Column(TypeName = "nvarchar(30)")]
    public CoachingRequestStatus Status { get; set; }
    public string CoachId { get; set; }
    public AppUser Coach { get; set; }
    public string ClientId { get; set; }
    public AppUser Client { get; set; }
    public int CourseId { get; set; }
    public TrainingCourse Course { get; set; }
    public int? Discount { get; set; }
    public string? VoucherCode { get; set; }
    public string? ClientMessage { get; set; }
    public string? RejectReason { get; set; }
    public string? CancelReason { get; set; }
}