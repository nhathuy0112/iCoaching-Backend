using System.ComponentModel.DataAnnotations;
using API.Attribute;

namespace API.Dto.Contract.TrainingLog;

public class TrainingLogUpdateRequest
{
    [Required(ErrorMessage = "Ngày tập luyện không được để trống")]
    [ValidTrainingLogDate]
    public string TrainingDate { get; set; }
    
    [Required(ErrorMessage = "Ghi chú không được để trống")]
    public string Note { get; set; }
    public IFormFile[]? Files { get; set; }
    public IFormFile[]? Images { get; set; }
    public IFormFile[]? Videos { get; set; }
}