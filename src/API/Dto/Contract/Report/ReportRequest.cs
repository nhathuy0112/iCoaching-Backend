using System.ComponentModel.DataAnnotations;

namespace API.Dto.Contract.Report;

public class ReportRequest
{
    [Required(ErrorMessage = "Vui lòng nhập lí do")]
    public string Desc { get; set; }
    [Required(ErrorMessage = "Vui lòng đính kèm hình ảnh")]
    public IFormFile[] Files { get; set; }
}