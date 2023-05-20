using Core.Entities.Auth;

namespace Core.Entities;

public class MediaAsset : BaseEntity
{
    public string Url { get; set; }
    public string PublicId { get; set; }
    public bool IsAvatar { get; set; }
    public string UserId { get; set; }
    public AppUser User { get; set; }
    public bool OnPortfolio { get; set; }
    public bool IsVideo { get; set; }
    public bool IsCert { get; set; }
    public int? CertificateSubmissionId { get; set; }
    public CertificateSubmission CertificateSubmission { get; set; }
    public int? ReportId { get; set; }
    public Report Report { get; set; }
    public int? TrainingLogId { get; set; }
    public TrainingLog TrainingLog { get; set; }
}