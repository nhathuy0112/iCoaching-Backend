using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Status;

namespace Core.Entities;

public class CertificateSubmission : BaseEntity
{
    public string CoachId { get; set; }
    public AppUser Coach { get; set; }
    [Column(TypeName = "nvarchar(20)")]
    public CertStatus Status { get; set; }

    public string? Reason { get; set; }
    public ICollection<MediaAsset> MediaAssets { get; set; }
}