using API.Dto.Contract.File;

namespace API.Dto.Contract.TrainingLog;

public class MediaOnLogList
{
    public int Id { get; set; }
    public string Url { get; set; }
}

public class TrainingLogResponse
{
    public int Id { get; set; }
    public int DateNo { get; set; }
    public string TrainingDate { get; set; }
    public string Status { get; set; }
    public string? Note { get; set; }
    public string? LastUpdatingDate { get; set; }
    public ICollection<FileResponse> Files { get; set; }
    public ICollection<MediaOnLogList> Images { get; set; }
    public ICollection<MediaOnLogList> Videos { get; set; }
}