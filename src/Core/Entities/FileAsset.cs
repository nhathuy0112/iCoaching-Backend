namespace Core.Entities;

public class FileAsset : BaseEntity
{
    public int ContractId { get; set; }
    public Contract Contract { get; set; }
    public string FileName { get; set; }
    public string DownloadUrl { get; set; }
    public DateTime Date  { get; set; }
    public long Size { get; set; }
    public int? TrainingLogId { get; set; }
    public TrainingLog TrainingLog { get; set; }
}