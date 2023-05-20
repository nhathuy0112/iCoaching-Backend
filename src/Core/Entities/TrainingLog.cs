using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Status;

namespace Core.Entities;

public class TrainingLog : BaseEntity
{
    public int ContractId { get; set; }
    public Contract Contract { get; set; }
    public int DateNo { get; set; }
    public DateTime? TrainingDate { get; set; }
    public DateTime? LastUpdatingDate { get; set; }
    public string? Note { get; set; }
    [Column(TypeName = "nvarchar(20)")]
    public TrainingLogStatus Status { get; set; }
    public ICollection<MediaAsset> MediaAssets { get; set; }
    public ICollection<FileAsset> FileAssets { get; set; }
}